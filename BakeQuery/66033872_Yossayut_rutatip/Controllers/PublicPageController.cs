using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _66033872_Yossayut_rutatip.Models.db;
using _66033872_Yossayut_rutatip.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace _66033872_Yossayut_rutatip.Controllers
{
    public class PublicPageController : Controller
    {
        private readonly Csi402BakequeryContext _db;
        private readonly IWebHostEnvironment _environment;

        public PublicPageController(Csi402BakequeryContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        public IActionResult Menu()
        {
            var products = _db.Products
                .Include(p => p.Category)
                .Where(p => p.Status != "INACTIVE" && p.StockQty > 0)
                .OrderByDescending(p => p.ProductId)
                .ToList();

            return View(products);
        }

        public IActionResult Promotion()
        {
            var promotions = _db.Promotions
                .Where(p => p.IsActive == true)
                .OrderByDescending(p => p.PromotionId)
                .Take(5)
                .ToList();

            return View(promotions);
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult AddToCart(ulong productId, int qty = 1, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _db.Products.FirstOrDefault(p =>
                p.ProductId == productId &&
                p.Status == "ACTIVE" &&
                p.StockQty > 0);

            if (product == null)
            {
                if (IsAjaxRequest())
                    return Json(new { success = false, message = "ไม่สามารถเพิ่มสินค้าได้" });
                TempData["CartToast"] = "ไม่สามารถเพิ่มสินค้าได้";
                return RedirectToLocal(returnUrl);
            }

            var cart = _db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId.Value && c.Status == "ACTIVE");

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId.Value,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Carts.Add(cart);
                _db.SaveChanges();
            }

            var safeQty = qty <= 0 ? 1 : qty;
            var existingItem = _db.CartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (existingItem == null)
            {
                existingItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Qty = Math.Min(safeQty, product.StockQty),
                    UnitPrice = product.Price,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.CartItems.Add(existingItem);
            }
            else
            {
                existingItem.Qty = Math.Min(existingItem.Qty + safeQty, product.StockQty);
                existingItem.UnitPrice = product.Price;
                existingItem.UpdatedAt = DateTime.Now;
            }

            cart.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["CartToast"] = $"เพิ่ม {product.Name} ลงตะกร้าเรียบร้อย";

            if (IsAjaxRequest())
            {
                var summary = BuildCartSummary(userId.Value);
                return Json(new { success = true, message = $"เพิ่ม {product.Name} ลงตะกร้าเรียบร้อย", totalQty = summary.TotalQty });
            }

            return RedirectToLocal(returnUrl);
        }

        [Authorize(Roles = "CUSTOMER")]
        public IActionResult Cart(string? promoCode = null, bool promoLocked = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var activeCart = _db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId.Value && c.Status == "ACTIVE");

            var vm = new CartPageViewModel();

            var defaultAddress = _db.UserAddresses
                .AsNoTracking()
                .FirstOrDefault(a => a.UserId == userId.Value && a.IsDefault);

            if (defaultAddress != null)
            {
                vm.HasDefaultAddress = true;
                vm.DefaultAddressDisplay = $"{defaultAddress.Line1} {defaultAddress.District} {defaultAddress.Province} {defaultAddress.PostalCode}".Trim();
            }

            if (activeCart == null)
            {
                return View(vm);
            }

            vm.PromoCode = string.IsNullOrWhiteSpace(promoCode) ? null : promoCode.Trim().ToUpperInvariant();
            var promoResult = EvaluatePromotionForCart(activeCart, userId.Value, vm.PromoCode, allowCartMutation: !promoLocked);

            if (promoResult.CartChanged)
            {
                return RedirectToAction("Cart", "PublicPage", new { promoCode = vm.PromoCode, promoLocked = true });
            }

            vm.Items = activeCart.CartItems
                .Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImageUrl = ci.Product.ImageUrl,
                    UnitPrice = ci.UnitPrice,
                    Qty = ci.Qty,
                    StockQty = ci.Product.StockQty
                })
                .ToList();

            vm.TotalQty = vm.Items.Sum(i => i.Qty);
            vm.Subtotal = vm.Items.Sum(i => i.LineTotal);
            vm.DiscountTotal = 0m;
            vm.ShippingFee = 0m;
            vm.DiscountTotal = promoResult.IsValid ? promoResult.DiscountValue : 0m;
            vm.IsPromoApplied = promoResult.IsValid;
            vm.PromoMessage = promoResult.Message;
            vm.GrandTotal = Math.Max(0m, vm.Subtotal - vm.DiscountTotal) + vm.ShippingFee;

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult UpdateCartItem(ulong cartItemId, int qty)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = _db.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId.Value && ci.Cart.Status == "ACTIVE");

            if (cartItem == null)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "ไม่พบสินค้าในตะกร้า" });
                }

                return RedirectToAction("Cart", "PublicPage");
            }

            if (qty <= 0)
            {
                _db.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Qty = Math.Min(qty, cartItem.Product.StockQty);
                cartItem.UpdatedAt = DateTime.Now;
            }

            cartItem.Cart.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            if (IsAjaxRequest())
            {
                var summary = BuildCartSummary(userId.Value);
                return Json(new
                {
                    success = true,
                    subtotal = summary.Subtotal,
                    totalQty = summary.TotalQty,
                    grandTotal = summary.GrandTotal,
                    lineTotal = qty <= 0 ? 0 : Math.Round(cartItem.UnitPrice * Math.Min(qty, cartItem.Product.StockQty), 2)
                });
            }

            return RedirectToAction("Cart", "PublicPage");
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult RemoveFromCart(ulong cartItemId)
        {
            return UpdateCartItem(cartItemId, 0);
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId.Value && c.Status == "ACTIVE");

            if (cart == null || !cart.CartItems.Any())
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = true, message = "ตะกร้าว่างอยู่แล้ว", totalQty = 0 });
                }

                TempData["CartToast"] = "ตะกร้าว่างอยู่แล้ว";
                return RedirectToAction("Cart", "PublicPage");
            }

            _db.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "ล้างรายการสินค้าเรียบร้อยแล้ว", totalQty = 0 });
            }

            TempData["CartToast"] = "ล้างรายการสินค้าเรียบร้อยแล้ว";
            return RedirectToAction("Cart", "PublicPage");
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult Checkout(CartPageViewModel data)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _db.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId.Value && c.Status == "ACTIVE");

            if (cart == null || cart.CartItems.Count == 0)
            {
                return RedirectToAction("Cart", "PublicPage");
            }

            var user = _db.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var address = _db.UserAddresses.FirstOrDefault(a => a.UserId == userId.Value && a.IsDefault);
            if (address == null)
            {
                TempData["OrderSuccess"] = "กรุณากรอกที่อยู่ในหน้า Profile ก่อนทำรายการสั่งซื้อ";
                return RedirectToAction("Profile", "Account");
            }

            var subtotal = cart.CartItems.Sum(ci => ci.Qty * ci.UnitPrice);
            var promoCode = string.IsNullOrWhiteSpace(data.PromoCode) ? null : data.PromoCode.Trim().ToUpperInvariant();
            var promoResult = EvaluatePromotionForCart(cart, userId.Value, promoCode, allowCartMutation: false);

            if (!string.IsNullOrWhiteSpace(promoCode) && !promoResult.IsValid)
            {
                TempData["OrderSuccess"] = promoResult.Message;
                return RedirectToAction("Cart", "PublicPage", new { promoCode });
            }

            var discountTotal = promoResult.IsValid ? promoResult.DiscountValue : 0m;
            var grandTotal = Math.Max(0m, subtotal - discountTotal);
            var orderNo = $"ORD{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(10, 99)}";

            var order = new Order
            {
                OrderNo = orderNo,
                UserId = userId.Value,
                AddressId = address.AddressId,
                PromotionId = promoResult.PromotionId,
                Subtotal = subtotal,
                DiscountTotal = discountTotal,
                ShippingFee = 0m,
                GrandTotal = grandTotal,
                OrderStatus = "PENDING",
                PaymentStatus = "UNPAID",
                Notes = string.IsNullOrWhiteSpace(data.CheckoutNote) ? null : data.CheckoutNote.Trim(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            foreach (var item in cart.CartItems)
            {
                var safeQty = Math.Min(item.Qty, item.Product.StockQty);
                if (safeQty <= 0)
                {
                    continue;
                }

                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Qty = safeQty,
                    UnitPrice = item.UnitPrice,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });

                item.Product.StockQty -= safeQty;
                item.Product.Status = item.Product.StockQty > 0 ? "ACTIVE" : "OUT_OF_STOCK";
                item.Product.UpdatedAt = DateTime.Now;
            }

            if (promoResult.IsValid && promoResult.PromotionId.HasValue && discountTotal > 0)
            {
                _db.PromotionRedemptions.Add(new PromotionRedemption
                {
                    PromotionId = promoResult.PromotionId.Value,
                    UserId = userId.Value,
                    OrderId = order.OrderId,
                    DiscountValue = discountTotal,
                    RedeemedAt = DateTime.Now
                });
            }

            cart.Status = "CHECKED_OUT";
            cart.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["OrderSuccess"] = $"สร้างคำสั่งซื้อ {order.OrderNo} เรียบร้อยแล้ว";
            return RedirectToAction("HomePage", "Account");
        }

        [Authorize(Roles = "CUSTOMER")]
        public IActionResult MyOrders()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderReplies)
                    .ThenInclude(r => r.RepliedByNavigation)
                .Include(o => o.PaymentProofs)
                .Where(o => o.UserId == userId.Value && o.OrderStatus != "DELIVERED" && o.OrderStatus != "CANCELLED")
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var vm = new CustomerOrderHistoryViewModel();
            vm.Orders = orders
                .Select(o => new CustomerOrderItemViewModel
                {
                    OrderId = o.OrderId,
                    OrderNo = o.OrderNo,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    GrandTotal = o.GrandTotal,
                    CreatedAt = o.CreatedAt,
                    CustomerNote = o.Notes,
                    Items = o.OrderItems.Select(oi => $"{oi.Product.Name} x{oi.Qty}").ToList(),
                    Replies = o.OrderReplies
                        .OrderByDescending(r => r.CreatedAt)
                        .Take(10)
                        .Select(r => new CustomerOrderReplyViewModel
                        {
                            RepliedBy = r.RepliedByNavigation != null ? r.RepliedByNavigation.FullName : "Staff",
                            Message = r.ReplyMessage,
                            CreatedAt = r.CreatedAt
                        })
                        .ToList(),
                    PaymentProofs = o.PaymentProofs
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(3)
                        .Select(p => new CustomerPaymentProofViewModel
                        {
                            FilePath = p.FilePath,
                            OriginalFileName = p.OriginalFileName,
                            VerificationStatus = p.VerificationStatus,
                            UploadNote = p.UploadNote,
                            CreatedAt = p.CreatedAt
                        })
                        .ToList()
                })
                .ToList();

            return View(vm);
        }

        [Authorize(Roles = "CUSTOMER")]
        public IActionResult OrderHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderReplies)
                    .ThenInclude(r => r.RepliedByNavigation)
                .Include(o => o.PaymentProofs)
                .Where(o => o.UserId == userId.Value && (o.OrderStatus == "DELIVERED" || o.OrderStatus == "CANCELLED"))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var vm = new CustomerOrderHistoryViewModel();
            vm.Orders = orders
                .Select(o => new CustomerOrderItemViewModel
                {
                    OrderId = o.OrderId,
                    OrderNo = o.OrderNo,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    GrandTotal = o.GrandTotal,
                    CreatedAt = o.CreatedAt,
                    CustomerNote = o.Notes,
                    Items = o.OrderItems.Select(oi => $"{oi.Product.Name} x{oi.Qty}").ToList(),
                    Replies = o.OrderReplies
                        .OrderByDescending(r => r.CreatedAt)
                        .Take(10)
                        .Select(r => new CustomerOrderReplyViewModel
                        {
                            RepliedBy = r.RepliedByNavigation != null ? r.RepliedByNavigation.FullName : "Staff",
                            Message = r.ReplyMessage,
                            CreatedAt = r.CreatedAt
                        })
                        .ToList(),
                    PaymentProofs = o.PaymentProofs
                        .OrderByDescending(p => p.CreatedAt)
                        .Take(3)
                        .Select(p => new CustomerPaymentProofViewModel
                        {
                            FilePath = p.FilePath,
                            OriginalFileName = p.OriginalFileName,
                            VerificationStatus = p.VerificationStatus,
                            UploadNote = p.UploadNote,
                            CreatedAt = p.CreatedAt
                        })
                        .ToList()
                })
                .ToList();

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult CancelOrder(ulong orderId, string? cancelReason)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId.Value);
            if (order == null)
            {
                TempData["OrderSuccess"] = "ไม่พบคำสั่งซื้อที่ต้องการยกเลิก";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            var normalizedStatus = (order.OrderStatus ?? string.Empty).Trim().ToUpperInvariant();
            var canCancel = normalizedStatus == "PENDING" || normalizedStatus == "PAID" || normalizedStatus == "PREPARING";
            if (!canCancel)
            {
                TempData["OrderSuccess"] = "ออเดอร์นี้ไม่สามารถยกเลิกได้แล้ว";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            var reason = string.IsNullOrWhiteSpace(cancelReason) ? null : cancelReason.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderSuccess"] = "กรุณาระบุเหตุผลในการยกเลิกออเดอร์";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            if (reason.Length > 500)
            {
                reason = reason[..500];
            }

            // คืน Stock เมื่อยกเลิกออเดอร์
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQty += item.Qty;
                    item.Product.Status = item.Product.StockQty > 0 ? "ACTIVE" : "OUT_OF_STOCK";
                    item.Product.UpdatedAt = DateTime.Now;
                }
            }

            order.OrderStatus = "CANCELLED";
            order.UpdatedAt = DateTime.Now;

            var notePrefix = "Cancellation Reason:";
            var sanitizedExistingNote = string.IsNullOrWhiteSpace(order.Notes)
                ? null
                : order.Notes.Trim();
            order.Notes = string.IsNullOrWhiteSpace(sanitizedExistingNote)
                ? $"{notePrefix} {reason}"
                : $"{sanitizedExistingNote}\n{notePrefix} {reason}";

            _db.OrderReplies.Add(new OrderReply
            {
                OrderId = order.OrderId,
                RepliedBy = userId.Value,
                ReplyMessage = $"ลูกค้ายกเลิกออเดอร์: {reason}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            _db.SaveChanges();

            TempData["OrderSuccess"] = $"ยกเลิกออเดอร์เรียบร้อยแล้ว ({order.OrderNo})";
            return RedirectToAction("OrderHistory", "PublicPage");
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult ConfirmReceived(ulong orderId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId.Value);
            if (order == null)
            {
                TempData["OrderSuccess"] = "ไม่พบคำสั่งซื้อที่ต้องการยืนยันรับสินค้า";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            var normalizedStatus = (order.OrderStatus ?? string.Empty).Trim().ToUpperInvariant();
            if (normalizedStatus != "SHIPPING")
            {
                TempData["OrderSuccess"] = "ออเดอร์นี้ยังไม่อยู่ในขั้นตอนจัดส่ง";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            order.OrderStatus = "DELIVERED";
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["OrderSuccess"] = $"ยืนยันรับสินค้าเรียบร้อยแล้ว ({order.OrderNo})";
            return RedirectToAction("OrderHistory", "PublicPage");
        }

        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        public IActionResult UploadPaymentProof(ulong orderId, IFormFile? paymentSlip, string? uploadNote)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId.Value);
            if (order == null)
            {
                TempData["OrderSuccess"] = "ไม่พบคำสั่งซื้อที่ต้องการอัปโหลดหลักฐาน";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            if (IsClosedOrder(order.OrderStatus))
            {
                TempData["OrderSuccess"] = "คำสั่งซื้อนี้สิ้นสุดแล้ว ไม่สามารถแนบหลักฐานเพิ่มได้";
                return RedirectToAction("OrderHistory", "PublicPage");
            }

            if (paymentSlip == null || paymentSlip.Length == 0)
            {
                TempData["OrderSuccess"] = "กรุณาแนบไฟล์หลักฐานการชำระเงิน";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            var extension = Path.GetExtension(paymentSlip.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".pdf" };
            if (!allowedExtensions.Contains(extension))
            {
                TempData["OrderSuccess"] = "รองรับเฉพาะไฟล์ .jpg, .jpeg, .png, .pdf เท่านั้น";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            if (paymentSlip.Length > 5 * 1024 * 1024)
            {
                TempData["OrderSuccess"] = "ไฟล์หลักฐานใหญ่เกินไป (ไม่เกิน 5MB)";
                return RedirectToAction("MyOrders", "PublicPage");
            }

            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", "payment-proofs");
            Directory.CreateDirectory(folderPath);

            var uniqueFileName = $"proof_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var fullFilePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                paymentSlip.CopyTo(stream);
            }

            var note = string.IsNullOrWhiteSpace(uploadNote) ? null : uploadNote.Trim();
            if (!string.IsNullOrWhiteSpace(note) && note.Length > 500)
            {
                note = note[..500];
            }

            var proof = new PaymentProof
            {
                OrderId = order.OrderId,
                UploadedBy = userId.Value,
                FilePath = $"/uploads/payment-proofs/{uniqueFileName}",
                OriginalFileName = paymentSlip.FileName,
                VerificationStatus = "PENDING",
                UploadNote = note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.PaymentProofs.Add(proof);
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            TempData["OrderSuccess"] = "แนบหลักฐานการชำระเงินเรียบร้อยแล้ว";
            return RedirectToAction("MyOrders", "PublicPage");
        }

        private ulong? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ulong.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Menu", "PublicPage");
        }

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private (int TotalQty, decimal Subtotal, decimal GrandTotal) BuildCartSummary(ulong userId)
        {
            var activeCart = _db.Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId && c.Status == "ACTIVE");

            if (activeCart == null)
            {
                return (0, 0m, 0m);
            }

            var totalQty = activeCart.CartItems.Sum(i => i.Qty);
            var subtotal = activeCart.CartItems.Sum(i => i.UnitPrice * i.Qty);
            var grandTotal = subtotal;
            return (totalQty, subtotal, grandTotal);
        }

        private static bool IsClosedOrder(string? status)
        {
            var normalized = status?.Trim().ToUpperInvariant();
            return normalized == "DELIVERED" || normalized == "CANCELLED";
        }

        private PromoEvaluationResult EvaluatePromotionForCart(Cart cart, ulong userId, string? promoCode, bool allowCartMutation)
        {
            if (string.IsNullOrWhiteSpace(promoCode))
            {
                return PromoEvaluationResult.Empty();
            }

            var now = DateTime.Now;
            var normalizedCode = promoCode.Trim().ToUpperInvariant();

            var promo = _db.Promotions
                .AsNoTracking()
                .Include(p => p.PromotionRules)
                .ToList()
                .FirstOrDefault(p =>
                    p.PromoCode.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase) ||
                    BuildCustomerPromotionCode(p).Equals(normalizedCode, StringComparison.OrdinalIgnoreCase));

            if (promo == null)
            {
                return PromoEvaluationResult.Invalid("ไม่พบ Promo Code นี้");
            }

            if (promo.IsActive != true || promo.StartAt > now || promo.EndAt < now)
            {
                return PromoEvaluationResult.Invalid("Promo Code นี้ยังไม่พร้อมใช้งาน");
            }

            var rule = promo.PromotionRules.FirstOrDefault();
            if (rule == null)
            {
                return PromoEvaluationResult.Invalid("ยังไม่ได้ตั้งค่าเงื่อนไขของโปรโมชั่นนี้");
            }

            var subtotal = cart.CartItems.Sum(ci => ci.Qty * ci.UnitPrice);

            if (rule.MinOrderAmount.HasValue && subtotal < rule.MinOrderAmount.Value)
            {
                return PromoEvaluationResult.Invalid($"ยอดสั่งซื้อไม่ถึงขั้นต่ำ ฿{rule.MinOrderAmount.Value:N2}");
            }

            if (rule.MemberOnly && !(User.Identity?.IsAuthenticated == true && User.IsInRole("CUSTOMER")))
            {
                return PromoEvaluationResult.Invalid("โปรนี้ใช้ได้เฉพาะสมาชิกเท่านั้น");
            }

            if (rule.MaxRedemptions.HasValue)
            {
                var redemptionCount = _db.PromotionRedemptions.Count(r => r.PromotionId == promo.PromotionId);
                if (redemptionCount >= rule.MaxRedemptions.Value)
                {
                    return PromoEvaluationResult.Invalid("Promo Code นี้ถูกใช้ครบจำนวนแล้ว");
                }
            }

            if (rule.MaxRedemptionsPerUser.HasValue)
            {
                var redemptionByUser = _db.PromotionRedemptions.Count(r => r.PromotionId == promo.PromotionId && r.UserId == userId);
                if (redemptionByUser >= rule.MaxRedemptionsPerUser.Value)
                {
                    return PromoEvaluationResult.Invalid("คุณใช้ Promo Code นี้ครบสิทธิ์แล้ว");
                }
            }

            decimal discount = 0m;
            var promoType = promo.PromoType.Trim().ToUpperInvariant();

            var cartChanged = false;

            if (promoType == "PERCENT")
            {
                if (!rule.DiscountPercent.HasValue || rule.DiscountPercent.Value <= 0)
                {
                    return PromoEvaluationResult.Invalid("ยังไม่ตั้งค่าเปอร์เซ็นต์ส่วนลด");
                }

                discount = subtotal * (rule.DiscountPercent.Value / 100m);
            }
            else if (promoType == "FIXED")
            {
                if (!rule.DiscountAmount.HasValue || rule.DiscountAmount.Value <= 0)
                {
                    return PromoEvaluationResult.Invalid("ยังไม่ตั้งค่าจำนวนเงินส่วนลด");
                }

                discount = rule.DiscountAmount.Value;
            }
            else if (promoType == "BUY_X_GET_Y")
            {
                if (!rule.BuyQty.HasValue || !rule.FreeQty.HasValue || !rule.FreeProductId.HasValue || rule.BuyQty.Value <= 0 || rule.FreeQty.Value <= 0)
                {
                    return PromoEvaluationResult.Invalid("เงื่อนไข Buy X Get Y ยังไม่ครบ");
                }

                var buyEligibleQty = cart.CartItems
                    .Where(ci => ci.ProductId != rule.FreeProductId.Value)
                    .Sum(ci => ci.Qty);

                if (buyEligibleQty < rule.BuyQty.Value)
                {
                    return PromoEvaluationResult.Invalid($"ต้องซื้อสินค้าให้ครบ {rule.BuyQty.Value} ชิ้นก่อน");
                }

                var freeItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == rule.FreeProductId.Value);
                var freeProduct = _db.Products.FirstOrDefault(p => p.ProductId == rule.FreeProductId.Value && p.Status != "INACTIVE");
                if (freeProduct == null)
                {
                    return PromoEvaluationResult.Invalid("สินค้าแถมของโปรนี้ไม่พร้อมใช้งาน");
                }

                if (allowCartMutation)
                {
                    var currentFreeQtyInCart = freeItem?.Qty ?? 0;
                    var requiredFreeQtyAfterApply = currentFreeQtyInCart + rule.FreeQty.Value;

                    if (requiredFreeQtyAfterApply > freeProduct.StockQty)
                    {
                        return PromoEvaluationResult.Invalid("คุกกี้สำหรับโปรโมชั่นมีสต็อกไม่พอ");
                    }

                    if (freeItem == null)
                    {
                        var bonusItem = new CartItem
                        {
                            CartId = cart.CartId,
                            ProductId = freeProduct.ProductId,
                            Qty = rule.FreeQty.Value,
                            UnitPrice = freeProduct.Price,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _db.CartItems.Add(bonusItem);
                        freeItem = bonusItem;
                    }
                    else
                    {
                        freeItem.Qty = requiredFreeQtyAfterApply;
                        freeItem.UnitPrice = freeProduct.Price;
                        freeItem.UpdatedAt = DateTime.Now;
                    }

                    cart.UpdatedAt = DateTime.Now;
                    _db.SaveChanges();
                    cartChanged = true;
                }

                if (freeItem == null)
                {
                    return PromoEvaluationResult.Invalid("กรุณา Apply โค้ดอีกครั้งเพื่อรับคุกกี้ฟรี");
                }

                discount = freeProduct.Price * rule.FreeQty.Value;
            }
            else if (promoType == "MEMBER")
            {
                discount = rule.DiscountPercent.HasValue && rule.DiscountPercent.Value > 0
                    ? subtotal * (rule.DiscountPercent.Value / 100m)
                    : (rule.DiscountAmount ?? 0m);
            }

            var currentSubtotal = cart.CartItems.Sum(ci => ci.Qty * ci.UnitPrice);
            discount = Math.Round(Math.Min(discount, currentSubtotal), 2);
            if (discount <= 0)
            {
                return PromoEvaluationResult.Invalid("โปรโมชั่นนี้ไม่สามารถใช้กับตะกร้าปัจจุบันได้");
            }

            return PromoEvaluationResult.Valid(promo.PromotionId, discount, $"ใช้โค้ด {promo.PromoCode} สำเร็จ ลด ฿{discount:N2}", cartChanged);
        }

        private static string BuildCustomerPromotionCode(Promotion promo)
        {
            var baseText = $"{promo.PromotionId}|{promo.PromoCode}|{promo.StartAt:yyyyMMdd}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(baseText));
            var token = Convert.ToHexString(hash)[..8];
            return $"BQ-{token}";
        }

        private sealed class PromoEvaluationResult
        {
            public bool IsValid { get; private set; }
            public ulong? PromotionId { get; private set; }
            public decimal DiscountValue { get; private set; }
            public string? Message { get; private set; }
            public bool CartChanged { get; private set; }

            public static PromoEvaluationResult Empty()
            {
                return new PromoEvaluationResult
                {
                    IsValid = false,
                    PromotionId = null,
                    DiscountValue = 0m,
                    Message = null,
                    CartChanged = false
                };
            }

            public static PromoEvaluationResult Invalid(string message)
            {
                return new PromoEvaluationResult
                {
                    IsValid = false,
                    PromotionId = null,
                    DiscountValue = 0m,
                    Message = message,
                    CartChanged = false
                };
            }

            public static PromoEvaluationResult Valid(ulong promotionId, decimal discountValue, string message, bool cartChanged)
            {
                return new PromoEvaluationResult
                {
                    IsValid = true,
                    PromotionId = promotionId,
                    DiscountValue = discountValue,
                    Message = message,
                    CartChanged = cartChanged
                };
            }
        }

    }
}
