using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _66033872_Yossayut_rutatip.Models;
using _66033872_Yossayut_rutatip.Models.db;
using _66033872_Yossayut_rutatip.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace _66033872_Yossayut_rutatip.Controllers;

[Authorize(Roles = "MANAGER,ADMIN,STAFF")]
public class BackOfficeController : Controller
{
    private readonly Csi402BakequeryContext _db;
    private readonly ILogger<BackOfficeController> _logger;
    private readonly IWebHostEnvironment _environment;

    public BackOfficeController(Csi402BakequeryContext db, ILogger<BackOfficeController> logger, IWebHostEnvironment environment)
    {
        _db = db;
        _logger = logger;
        _environment = environment;
    }

    [Authorize(Roles = "MANAGER")]
    public IActionResult AdminPage()
    {
        var now = DateTime.Now;
        var startOfToday = now.Date;
        var startOfTomorrow = startOfToday.AddDays(1);
        var trendStartDate = startOfToday.AddDays(-13);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfNextMonth = startOfMonth.AddMonths(1);
        var startOfYear = new DateTime(now.Year, 1, 1);
        var startOfNextYear = startOfYear.AddYears(1);

        var successfulOrders = _db.Orders
            .AsNoTracking()
            .Where(o =>
                o.OrderStatus.ToUpper() != "PENDING" &&
                o.OrderStatus.ToUpper() != "CANCELLED" &&
                (
                    o.PaymentStatus.ToUpper() == "PAID" ||
                    o.PaymentProofs.Any(p => p.VerificationStatus.ToUpper() == "APPROVED")
                ));

        var data = new DashboardViewModel
        {
            SalesToday = successfulOrders
                .Where(o => o.CreatedAt >= startOfToday && o.CreatedAt < startOfTomorrow)
                .Select(o => (decimal?)o.GrandTotal)
                .Sum() ?? 0m,

            SalesThisMonth = successfulOrders
                .Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt < startOfNextMonth)
                .Select(o => (decimal?)o.GrandTotal)
                .Sum() ?? 0m,

            SalesThisYear = successfulOrders
                .Where(o => o.CreatedAt >= startOfYear && o.CreatedAt < startOfNextYear)
                .Select(o => (decimal?)o.GrandTotal)
                .Sum() ?? 0m
        };

        var trendRaw = successfulOrders
            .Where(o => o.CreatedAt >= trendStartDate && o.CreatedAt < startOfTomorrow)
            .Select(o => new { o.CreatedAt, o.GrandTotal })
            .ToList();

        var trendMap = trendRaw
            .GroupBy(x => x.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.GrandTotal));

        data.SalesTrend = Enumerable.Range(0, 14)
            .Select(offset =>
            {
                var date = trendStartDate.AddDays(offset);
                return new SalesTrendPointViewModel
                {
                    Label = date.ToString("dd/MM"),
                    Amount = trendMap.TryGetValue(date, out var total) ? total : 0m
                };
            })
            .ToList();

        data.TopSellingProducts = _db.OrderItems
            .AsNoTracking()
            .Where(oi =>
                oi.Order.OrderStatus.ToUpper() != "PENDING" &&
                oi.Order.OrderStatus.ToUpper() != "CANCELLED")
            .Where(oi =>
                oi.Order.PaymentStatus.ToUpper() == "PAID" ||
                oi.Order.PaymentProofs.Any(p => p.VerificationStatus.ToUpper() == "APPROVED"))
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopSellingProductViewModel
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(x => x.Qty),
                Revenue = g.Sum(x => (decimal?)((x.LineTotal ?? 0m) > 0m
                    ? x.LineTotal
                    : x.UnitPrice * x.Qty)) ?? 0m
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenByDescending(x => x.Revenue)
            .Take(7)
            .ToList();

        var rolePalette = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["MANAGER"] = "#0d6efd",
            ["ADMIN"] = "#fd7e14",
            ["STAFF"] = "#20c997",
            ["CUSTOMER"] = "#dc3545"
        };

        data.UserRoleDistribution = _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.RoleId)
            .Select(r => new RoleDistributionViewModel
            {
                RoleName = r.RoleName,
                UserCount = r.Users.Count(),
                ColorHex = rolePalette.ContainsKey(r.RoleName) ? rolePalette[r.RoleName] : "#6c757d"
            })
            .ToList();

        return View(data);
    }
    
    [Authorize(Roles = "MANAGER,ADMIN,STAFF")]
    public IActionResult OrderMenu(string? timeFilter)
    {
        var data = new OrderManagementViewModel();

        var normalizedFilter = string.IsNullOrWhiteSpace(timeFilter)
            ? "day"
            : timeFilter.Trim().ToLowerInvariant();

        var allowedFilters = new HashSet<string> { "day", "week", "month", "year" };
        if (!allowedFilters.Contains(normalizedFilter))
        {
            normalizedFilter = "day";
        }

        var now = DateTime.Now;
        DateTime start;
        DateTime end;

        switch (normalizedFilter)
        {
            case "week":
                var diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                start = now.Date.AddDays(-diff);
                end = start.AddDays(7);
                break;
            case "month":
                start = new DateTime(now.Year, now.Month, 1);
                end = start.AddMonths(1);
                break;
            case "year":
                start = new DateTime(now.Year, 1, 1);
                end = start.AddYears(1);
                break;
            default:
                start = now.Date;
                end = start.AddDays(1);
                break;
        }

        var orders = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderReplies)
                .ThenInclude(r => r.RepliedByNavigation)
            .Include(o => o.PaymentProofs)
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
            .OrderByDescending(o => o.CreatedAt)
            .Take(200)
            .ToList();

        data.SelectedTimeFilter = normalizedFilter;

        data.Orders = orders
            .Select(o => new OrderManagementItemViewModel
            {
                OrderId = o.OrderId,
                OrderNo = o.OrderNo,
                CustomerName = o.User.FullName,
                CustomerPhone = o.Address.Phone,
                CustomerAddress = string.Join(", ", new[]
                {
                    o.Address.Line1,
                    o.Address.Line2,
                    o.Address.District,
                    o.Address.Province,
                    o.Address.PostalCode,
                    o.Address.Country
                }.Where(part => !string.IsNullOrWhiteSpace(part))),
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                GrandTotal = o.GrandTotal,
                CreatedAt = o.CreatedAt,
                CustomerNote = o.Notes,
                ItemNames = o.OrderItems.Select(oi => $"{oi.Product.Name} x{oi.Qty}").ToList(),
                Replies = o.OrderReplies
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new OrderReplyItemViewModel
                    {
                        RepliedByName = r.RepliedByNavigation != null ? r.RepliedByNavigation.FullName : "Staff",
                        ReplyMessage = r.ReplyMessage,
                        CreatedAt = r.CreatedAt
                    })
                    .ToList(),
                LatestPaymentProof = o.PaymentProofs
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new OrderPaymentProofViewModel
                    {
                        FilePath = p.FilePath,
                        OriginalFileName = p.OriginalFileName,
                        VerificationStatus = p.VerificationStatus,
                        UploadNote = p.UploadNote,
                        CreatedAt = p.CreatedAt
                    })
                    .FirstOrDefault()
            })
            .ToList();

        return View(data);
    }

    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult MenuProducts(ushort? categoryId, string? status, string? keyword, string? sortBy)
    {
        SyncProductStatusByStock();

        var data = new MenuProductsViewModel();

        data.SelectedCategoryId = categoryId;
        data.SelectedStatus = status;
        data.Keyword = keyword;
        data.SortBy = string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy.Trim().ToLowerInvariant();

        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? string.Empty
            : status.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(p => p.Status.ToUpper() == normalizedStatus);
        }

        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword)
            ? string.Empty
            : keyword.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            query = query.Where(p =>
                p.Name.ToLower().Contains(normalizedKeyword) ||
                p.ProductCode.ToLower().Contains(normalizedKeyword));
        }

        query = data.SortBy switch
        {
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "stock_asc" => query.OrderBy(p => p.StockQty),
            "stock_desc" => query.OrderByDescending(p => p.StockQty),
            "oldest" => query.OrderBy(p => p.ProductId),
            _ => query.OrderByDescending(p => p.ProductId)
        };

        data.ProductList = query.ToList();

        data.CategoryList = _db.Categories
            .OrderBy(c => c.Name)
            .ToList();

        return View(data);
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> MenuProducts(MenuProductsViewModel data)
    {
        var input = data.ProductForm;

        if (string.IsNullOrWhiteSpace(input.Name) || input.Price <= 0)
        {
            return RedirectToAction("MenuProducts", "BackOffice");
        }

        var categoryId = input.CategoryId;
        if (categoryId == 0)
        {
            categoryId = _db.Categories.OrderBy(c => c.CategoryId).Select(c => c.CategoryId).FirstOrDefault();
        }

        var standardCodePattern = new Regex("^P\\d{3}$", RegexOptions.Compiled);
        var nextCodeNumber = _db.Products
            .Select(item => item.ProductCode)
            .AsEnumerable()
            .Where(code => !string.IsNullOrWhiteSpace(code) && standardCodePattern.IsMatch(code))
            .Select(code => int.Parse(code![1..]))
            .DefaultIfEmpty(0)
            .Max() + 1;

        var productCode = $"P{nextCodeNumber:D3}";
        var p = new Product();
        p.ProductCode = productCode;
        p.CategoryId = categoryId;
        p.Name = input.Name.Trim();
        p.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        p.Price = input.Price;
        p.StockQty = input.StockQty;

        // Handle image upload
        if (input.ImageFile != null && input.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + input.ImageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await input.ImageFile.CopyToAsync(fileStream);
            }
            p.ImageUrl = "/uploads/" + uniqueFileName;
        }
        else
        {
            p.ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim();
        }

        p.Status = input.StockQty > 0 ? "ACTIVE" : "OUT_OF_STOCK";
        p.CreatedAt = DateTime.Now;
        p.UpdatedAt = DateTime.Now;

        _db.Products.Add(p);
        _db.SaveChanges();

        return RedirectToAction("MenuProducts", "BackOffice");
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public async Task<IActionResult> UpdateProduct(MenuProductsViewModel data)
    {
        var input = data.EditProductForm;
        var product = _db.Products.FirstOrDefault(p => p.ProductId == input.ProductId);

        if (product == null)
        {
            return RedirectToAction("MenuProducts", "BackOffice");
        }

        if (string.IsNullOrWhiteSpace(input.Name) || input.Price <= 0)
        {
            return RedirectToAction("MenuProducts", "BackOffice");
        }

        var categoryId = input.CategoryId;
        if (!_db.Categories.Any(c => c.CategoryId == categoryId))
        {
            categoryId = _db.Categories.OrderBy(c => c.CategoryId).Select(c => c.CategoryId).FirstOrDefault();
        }

        product.Name = input.Name.Trim();
        product.CategoryId = categoryId;
        product.Price = input.Price;
        product.StockQty = input.StockQty;

        // Handle image upload
        if (input.ImageFile != null && input.ImageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + input.ImageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await input.ImageFile.CopyToAsync(fileStream);
            }
            product.ImageUrl = "/uploads/" + uniqueFileName;
        }
        else if (!string.IsNullOrWhiteSpace(input.ImageUrl))
        {
            product.ImageUrl = input.ImageUrl.Trim();
        }
        // else keep existing ImageUrl

        product.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        product.Status = string.IsNullOrWhiteSpace(input.Status)
            ? (product.StockQty > 0 ? "ACTIVE" : "OUT_OF_STOCK")
            : input.Status.Trim().ToUpper();

        if (product.StockQty <= 0)
        {
            product.Status = "OUT_OF_STOCK";
        }
        else if (product.Status == "OUT_OF_STOCK")
        {
            product.Status = "ACTIVE";
        }

        product.UpdatedAt = DateTime.Now;

        _db.SaveChanges();

        return RedirectToAction("MenuProducts", "BackOffice");
    }

    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult DeleteProduct(ulong productId)
    {
        var product = (from p in _db.Products where p.ProductId == productId select p).FirstOrDefault();

        _db.RemoveRange(product);
        _db.SaveChanges();
        return RedirectToAction("MenuProducts", "BackOffice");
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult CreateCategory(MenuProductsViewModel data)
    {
        var input = data.CategoryForm;
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return RedirectToAction("MenuProducts", "BackOffice");
        }

        var categoryName = input.Name.Trim();
        var isDuplicateName = _db.Categories.Any(c => c.Name.ToLower() == categoryName.ToLower());
        if (isDuplicateName)
        {
            return RedirectToAction("MenuProducts", "BackOffice");
        }

        var categoryCodePattern = new Regex("^C\\d{3}$", RegexOptions.Compiled);
        var nextCategoryNumber = _db.Categories
            .Select(item => item.CategoryCode)
            .AsEnumerable()
            .Where(code => !string.IsNullOrWhiteSpace(code) && categoryCodePattern.IsMatch(code))
            .Select(code => int.Parse(code![1..]))
            .DefaultIfEmpty(0)
            .Max() + 1;

        var maxSortOrder = _db.Categories.Any()
            ? _db.Categories.Max(c => c.SortOrder)
            : 0;
        var nextSortOrder = maxSortOrder + 1;

        var category = new Category();
        category.CategoryCode = $"C{nextCategoryNumber:D3}";
        category.Name = categoryName;
        category.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        category.IsActive = input.IsActive;
        category.SortOrder = nextSortOrder;
        category.CreatedAt = DateTime.Now;
        category.UpdatedAt = DateTime.Now;

        _db.Categories.Add(category);
        _db.SaveChanges();

        return RedirectToAction("MenuProducts", "BackOffice");
    }

    private void SyncProductStatusByStock()
    {
        var productsToSync = _db.Products
            .Where(p =>
                (p.StockQty > 0 && p.Status == "OUT_OF_STOCK") ||
                (p.StockQty <= 0 && p.Status == "ACTIVE"))
            .ToList();

        if (productsToSync.Count == 0)
        {
            return;
        }

        foreach (var product in productsToSync)
        {
            product.Status = product.StockQty > 0 ? "ACTIVE" : "OUT_OF_STOCK";
            product.UpdatedAt = DateTime.Now;
        }

        _db.SaveChanges();
    }

    [Authorize(Roles = "MANAGER")]
    public IActionResult AccountManagement(byte? roleId, string? keyword, string? sortBy)
    {
        var data = new AccountManagementViewModel();

        var roleCountMap = _db.Roles
            .AsNoTracking()
            .Select(r => new
            {
                RoleName = r.RoleName.ToUpper(),
                UserCount = r.Users.Count()
            })
            .ToDictionary(x => x.RoleName, x => x.UserCount);

        data.ManagerCount = roleCountMap.TryGetValue("MANAGER", out var managerCount) ? managerCount : 0;
        data.AdminCount = roleCountMap.TryGetValue("ADMIN", out var adminCount) ? adminCount : 0;
        data.StaffCount = roleCountMap.TryGetValue("STAFF", out var staffCount) ? staffCount : 0;
        data.CustomerCount = roleCountMap.TryGetValue("CUSTOMER", out var customerCount) ? customerCount : 0;

        data.SelectedRoleId = roleId;
        data.Keyword = keyword;
        data.SortBy = string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy.Trim().ToLowerInvariant();

        var users = _db.Users
            .Include(u => u.Role)
            .AsQueryable();

        if (roleId.HasValue)
        {
            users = users.Where(u => u.RoleId == roleId.Value);
        }

        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword)
            ? string.Empty
            : keyword.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            users = users.Where(u =>
                u.FullName.ToLower().Contains(normalizedKeyword) ||
                u.Email.ToLower().Contains(normalizedKeyword) ||
                u.UserCode.ToLower().Contains(normalizedKeyword));
        }

        users = data.SortBy switch
        {
            "name_asc" => users.OrderBy(u => u.FullName),
            "name_desc" => users.OrderByDescending(u => u.FullName),
            "role_asc" => users.OrderBy(u => u.Role.RoleName).ThenBy(u => u.FullName),
            "role_desc" => users.OrderByDescending(u => u.Role.RoleName).ThenBy(u => u.FullName),
            "oldest" => users.OrderBy(u => u.UserId),
            _ => users.OrderByDescending(u => u.UserId)
        };

        data.UserList = users
            .Select(u => new AuthUserViewModel
            {
                UserId = u.UserId,
                UserCode = u.UserCode,
                RoleId = u.RoleId,
                RoleName = u.Role.RoleName,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Status = u.Status,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToList();

        data.RoleList = _db.Roles
            .OrderBy(r => r.RoleId)
            .ToList();

        return View(data);
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public IActionResult ChangeAccountPassword(ulong userId, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "รหัสผ่านใหม่ต้องมีอย่างน้อย 6 ตัวอักษร";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "ยืนยันรหัสผ่านไม่ตรงกัน";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "ไม่พบบัญชีผู้ใช้ที่ต้องการเปลี่ยนรหัสผ่าน";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        user.PasswordHash = newPassword.Trim();
        user.UpdatedAt = DateTime.Now;
        _db.SaveChanges();

        TempData["AccountToastType"] = "success";
        TempData["AccountToastMessage"] = $"เปลี่ยนรหัสผ่านให้ {user.FullName} เรียบร้อยแล้ว";
        return RedirectToAction("AccountManagement", "BackOffice");
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public IActionResult AccountManagement(AccountManagementViewModel data)
    {
        var input = data.AccountForm;
        if (string.IsNullOrWhiteSpace(input.FullName) ||
            string.IsNullOrWhiteSpace(input.Email) ||
            string.IsNullOrWhiteSpace(input.Password))
        {
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        if (!IsValidPhone(input.Phone))
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "เบอร์โทรต้องเป็นตัวเลขเท่านั้น และไม่เกิน 10 หลัก";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        var email = input.Email.Trim();
        var isDuplicateEmail = _db.Users.Any(u => u.Email.ToLower() == email.ToLower());
        if (isDuplicateEmail)
        {
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        var standardCodePattern = new Regex("^U\\d+$", RegexOptions.Compiled);
        var nextCodeNumber = _db.Users
            .Select(item => item.UserCode)
            .AsEnumerable()
            .Where(code => !string.IsNullOrWhiteSpace(code) && standardCodePattern.IsMatch(code))
            .Select(code => ulong.TryParse(code![1..], out var n) ? n : 0UL)
            .DefaultIfEmpty(0UL)
            .Max() + 1;

        var roleId = input.RoleId;
        if (!_db.Roles.Any(r => r.RoleId == roleId))
        {
            roleId = _db.Roles.OrderBy(r => r.RoleId).Select(r => r.RoleId).FirstOrDefault();
        }

        var user = new User();
        user.UserCode = $"U{nextCodeNumber:D3}";
        user.RoleId = roleId;
        user.Email = email;
        user.PasswordHash = input.Password;
        user.FullName = input.FullName.Trim();
        user.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        user.Status = string.IsNullOrWhiteSpace(input.Status) ? "ACTIVE" : input.Status.Trim().ToUpper();
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        _db.Users.Add(user);
        _db.SaveChanges();

        return RedirectToAction("AccountManagement", "BackOffice");
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER")]
    public IActionResult UpdateAccount(AccountManagementViewModel data)
    {
        var input = data.EditAccountForm;
        var user = _db.Users.FirstOrDefault(u => u.UserId == input.UserId);

        if (user == null)
        {
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        if (string.IsNullOrWhiteSpace(input.FullName) || string.IsNullOrWhiteSpace(input.Email))
        {
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        if (!IsValidPhone(input.Phone))
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "เบอร์โทรต้องเป็นตัวเลขเท่านั้น และไม่เกิน 10 หลัก";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        var email = input.Email.Trim();
        var isDuplicateEmail = _db.Users.Any(u => u.UserId != input.UserId && u.Email.ToLower() == email.ToLower());
        if (isDuplicateEmail)
        {
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        var roleId = input.RoleId;
        if (!_db.Roles.Any(r => r.RoleId == roleId))
        {
            roleId = _db.Roles.OrderBy(r => r.RoleId).Select(r => r.RoleId).FirstOrDefault();
        }

        var managerRoleId = _db.Roles
            .Where(r => r.RoleName.ToUpper() == "MANAGER")
            .Select(r => (byte?)r.RoleId)
            .FirstOrDefault();

        var currentUserId = GetCurrentUserId();
        var isEditingSelf = currentUserId.HasValue && currentUserId.Value == user.UserId;
        var isTargetManager = managerRoleId.HasValue && user.RoleId == managerRoleId.Value;
        var isChangingRoleFromManager = managerRoleId.HasValue && roleId != managerRoleId.Value;

        if (isEditingSelf && isTargetManager && isChangingRoleFromManager)
        {
            var hasAnotherManager = _db.Users.Any(u =>
                u.UserId != user.UserId &&
                managerRoleId.HasValue &&
                u.RoleId == managerRoleId.Value);

            if (!hasAnotherManager)
            {
                TempData["AccountToastType"] = "danger";
                TempData["AccountToastMessage"] = "ยังไม่สามารถเปลี่ยน Role ตัวเองได้ ต้องมี Manager คนอื่นอย่างน้อย 1 บัญชีก่อน";
                return RedirectToAction("AccountManagement", "BackOffice");
            }
        }

        user.RoleId = roleId;
        user.FullName = input.FullName.Trim();
        user.Email = email;
        user.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        user.Status = string.IsNullOrWhiteSpace(input.Status) ? "ACTIVE" : input.Status.Trim().ToUpper();
        user.UpdatedAt = DateTime.Now;

        _db.SaveChanges();

        return RedirectToAction("AccountManagement", "BackOffice");
    }

    [Authorize(Roles = "MANAGER")]
    public IActionResult DeleteAccount(ulong userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId.HasValue && currentUserId.Value == userId)
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "ไม่สามารถลบบัญชีของตัวเองได้";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            TempData["AccountToastType"] = "danger";
            TempData["AccountToastMessage"] = "ไม่พบบัญชีผู้ใช้ที่ต้องการลบ";
            return RedirectToAction("AccountManagement", "BackOffice");
        }

        _db.Users.Remove(user);
        _db.SaveChanges();
        return RedirectToAction("AccountManagement", "BackOffice");
    }


    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult Promotions()
    {
        var data = new PromotionsPageViewModel();
        data.PromotionList = _db.Promotions
            .AsNoTracking()
            .Include(p => p.PromotionRules)
                .ThenInclude(r => r.FreeProduct)
            .Include(p => p.PromotionRedemptions)
            .OrderByDescending(p => p.PromotionId)
            .Select(p => new PromotionListItemViewModel
            {
                PromotionId = p.PromotionId,
                PromoCode = p.PromoCode,
                CustomerCode = BuildCustomerPromotionCode(p),
                Name = p.Name,
                PromoType = p.PromoType,
                Description = p.Description,
                StartAt = NormalizePromotionDate(p.StartAt),
                EndAt = NormalizePromotionDate(p.EndAt),
                IsActive = p.IsActive ?? false,
                MinOrderAmount = p.PromotionRules.Select(r => r.MinOrderAmount).FirstOrDefault(),
                DiscountPercent = p.PromotionRules.Select(r => r.DiscountPercent).FirstOrDefault(),
                DiscountAmount = p.PromotionRules.Select(r => r.DiscountAmount).FirstOrDefault(),
                BuyQty = p.PromotionRules.Select(r => r.BuyQty).FirstOrDefault(),
                FreeQty = p.PromotionRules.Select(r => r.FreeQty).FirstOrDefault(),
                FreeProductId = p.PromotionRules.Select(r => r.FreeProductId).FirstOrDefault(),
                FreeProductName = p.PromotionRules.Select(r => r.FreeProduct != null ? r.FreeProduct.Name : null).FirstOrDefault(),
                MemberOnly = p.PromotionRules.Select(r => r.MemberOnly).FirstOrDefault(),
                MaxRedemptions = p.PromotionRules.Select(r => r.MaxRedemptions).FirstOrDefault(),
                MaxRedemptionsPerUser = p.PromotionRules.Select(r => r.MaxRedemptionsPerUser).FirstOrDefault(),
                RedemptionCount = p.PromotionRedemptions.Count
            })
            .ToList();

        data.ProductOptions = _db.Products
            .AsNoTracking()
            .Where(p => p.Status != "INACTIVE")
            .OrderBy(p => p.Name)
            .Select(p => new ProductOptionViewModel
            {
                ProductId = p.ProductId,
                Name = p.Name
            })
            .ToList();

        data.PromotionForm.PromoType = "PERCENT";
        data.PromotionForm.StartAt = DateTime.Now;
        data.PromotionForm.EndAt = DateTime.Now.AddDays(7);
        data.PromotionForm.MinOrderAmount = 0m;
        data.PromotionForm.MaxRedemptions = 0;
        data.PromotionForm.MaxRedemptionsPerUser = 0;

        return View(data);
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult Promotions(PromotionsPageViewModel data)
    {
        var input = data.PromotionForm;
        var normalizedPromoType = NormalizePromoType(input.PromoType);
        var normalizedStartAt = NormalizePromotionDate(input.StartAt);
        var normalizedEndAt = NormalizePromotionDate(input.EndAt);

        if (string.IsNullOrWhiteSpace(input.Name) ||
            string.IsNullOrWhiteSpace(normalizedPromoType) ||
            normalizedEndAt <= normalizedStartAt)
        {
            return RedirectToAction("Promotions", "BackOffice");
        }

        var promoCode = string.IsNullOrWhiteSpace(input.PromoCode)
            ? BuildNextPromotionCode()
            : input.PromoCode.Trim().ToUpperInvariant();

        if (_db.Promotions.Any(p => p.PromoCode == promoCode))
        {
            TempData["OrderSuccess"] = "Promo Code นี้มีอยู่แล้ว";
            return RedirectToAction("Promotions", "BackOffice");
        }

        if (!IsValidPromotionRule(input, normalizedPromoType))
        {
            TempData["OrderSuccess"] = "กรุณาตรวจสอบเงื่อนไข Promotion Rule";
            return RedirectToAction("Promotions", "BackOffice");
        }

        var currentUserId = GetCurrentUserId();
        var now = DateTime.Now;

        var promo = new Promotion();
        promo.PromoCode = promoCode;
        promo.Name = input.Name.Trim();
        promo.PromoType = normalizedPromoType;
        promo.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        promo.StartAt = normalizedStartAt;
        promo.EndAt = normalizedEndAt;
        promo.IsActive = input.IsActive;
        promo.CreatedBy = currentUserId;
        promo.UpdatedBy = currentUserId;
        promo.CreatedAt = now;
        promo.UpdatedAt = now;

        _db.Promotions.Add(promo);
        _db.SaveChanges();

        UpsertPromotionRule(promo.PromotionId, input, now);
        _db.SaveChanges();

        return RedirectToAction("Promotions", "BackOffice");
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult UpdatePromotion(PromotionsPageViewModel data)
    {
        var input = data.EditPromotionForm;
        var promo = _db.Promotions.FirstOrDefault(p => p.PromotionId == input.PromotionId);
        var normalizedPromoType = NormalizePromoType(input.PromoType);
        var normalizedStartAt = NormalizePromotionDate(input.StartAt);
        var normalizedEndAt = NormalizePromotionDate(input.EndAt);

        if (promo == null)
        {
            return RedirectToAction("Promotions", "BackOffice");
        }

        if (string.IsNullOrWhiteSpace(input.Name) ||
            string.IsNullOrWhiteSpace(normalizedPromoType) ||
            normalizedEndAt <= normalizedStartAt)
        {
            return RedirectToAction("Promotions", "BackOffice");
        }

        if (!IsValidPromotionRule(input, normalizedPromoType))
        {
            TempData["OrderSuccess"] = "กรุณาตรวจสอบเงื่อนไข Promotion Rule";
            return RedirectToAction("Promotions", "BackOffice");
        }

        var currentUserId = GetCurrentUserId();
        var now = DateTime.Now;

        promo.Name = input.Name.Trim();
        promo.PromoType = normalizedPromoType;
        promo.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        promo.StartAt = normalizedStartAt;
        promo.EndAt = normalizedEndAt;
        promo.IsActive = input.IsActive;
        promo.UpdatedBy = currentUserId;
        promo.UpdatedAt = now;

        UpsertPromotionRule(promo.PromotionId, input, now);

        _db.SaveChanges();

        return RedirectToAction("Promotions", "BackOffice");
    }

    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult DeletePromotion(ulong promotionId)
    {
        var promotion = _db.Promotions
            .Include(p => p.Orders)
            .Include(p => p.PromotionRedemptions)
            .FirstOrDefault(p => p.PromotionId == promotionId);

        if (promotion == null)
        {
            return RedirectToAction("Promotions", "BackOffice");
        }

        var hasUsage = promotion.Orders.Any() || promotion.PromotionRedemptions.Any();
        if (hasUsage)
        {
            promotion.IsActive = false;
            promotion.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
            TempData["OrderSuccess"] = "โปรโมชันนี้ถูกใช้งานแล้ว ระบบจะปิดการใช้งานแทนการลบ";
            return RedirectToAction("Promotions", "BackOffice");
        }

        var ruleList = _db.PromotionRules.Where(r => r.PromotionId == promotionId).ToList();
        if (ruleList.Count > 0)
        {
            _db.PromotionRules.RemoveRange(ruleList);
        }

        _db.Promotions.Remove(promotion);
        _db.SaveChanges();
        return RedirectToAction("Promotions", "BackOffice");
    }

    [HttpGet]
    [Authorize(Roles = "MANAGER,ADMIN")]
    public IActionResult GetPromotionUsage(ulong promotionId)
    {
        var promotion = _db.Promotions
            .AsNoTracking()
            .Where(p => p.PromotionId == promotionId)
            .Select(p => new { p.PromotionId, p.PromoCode, p.Name })
            .FirstOrDefault();

        if (promotion == null)
        {
            return NotFound(new { success = false, message = "ไม่พบโปรโมชั่นที่ต้องการ" });
        }

        var usageByUser = _db.PromotionRedemptions
            .AsNoTracking()
            .Where(r => r.PromotionId == promotionId)
            .GroupBy(r => new
            {
                r.UserId,
                r.User.UserCode,
                r.User.FullName,
                r.User.Email
            })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.UserCode,
                g.Key.FullName,
                g.Key.Email,
                UseCount = g.Count(),
                TotalDiscount = g.Sum(x => x.DiscountValue),
                LastUsedAt = g.Max(x => x.RedeemedAt)
            })
            .OrderByDescending(x => x.UseCount)
            .ThenByDescending(x => x.LastUsedAt)
            .ToList()
            .Select(x => new
            {
                x.UserId,
                x.UserCode,
                x.FullName,
                x.Email,
                x.UseCount,
                x.TotalDiscount,
                LastUsedAt = x.LastUsedAt.ToString("dd/MM/yyyy HH:mm")
            })
            .ToList();

        var totalRedemptions = usageByUser.Sum(x => x.UseCount);

        return Json(new
        {
            success = true,
            promotion = new
            {
                promotion.PromotionId,
                promotion.PromoCode,
                promotion.Name
            },
            totalRedemptions,
            uniqueUsers = usageByUser.Count,
            usageByUser
        });
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN,STAFF")]
    public IActionResult UpdateOrderStatus(ulong orderId, string orderStatus, string? timeFilter)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
        }

        var normalizedStatus = (orderStatus ?? string.Empty).Trim().ToUpperInvariant();
        var allowedStatuses = new HashSet<string>
        {
            "PENDING",
            "PAID",
            "PREPARING",
            "SHIPPING",
            "DELIVERED",
            "CANCELLED"
        };

        if (!allowedStatuses.Contains(normalizedStatus))
        {
            return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
        }

        order.OrderStatus = normalizedStatus;
        order.UpdatedAt = DateTime.Now;
        _db.SaveChanges();

        return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN,STAFF")]
    public IActionResult UpdatePaymentStatus(ulong orderId, string paymentStatus, string? timeFilter)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
        }

        var normalizedStatus = (paymentStatus ?? string.Empty).Trim().ToUpperInvariant();
        var allowedStatuses = new HashSet<string>
        {
            "UNPAID",
            "PAID",
            "REFUNDED"
        };

        if (!allowedStatuses.Contains(normalizedStatus))
        {
            return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
        }

        var latestProof = _db.PaymentProofs
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.ProofId)
            .FirstOrDefault();

        order.PaymentStatus = normalizedStatus;

        if (normalizedStatus == "PAID" && order.OrderStatus == "PENDING")
        {
            order.OrderStatus = "PAID";
        }

        if (normalizedStatus == "PAID" && latestProof != null)
        {
            latestProof.VerificationStatus = "APPROVED";
            latestProof.UpdatedAt = DateTime.Now;
        }

        if (normalizedStatus == "UNPAID" && order.OrderStatus == "PAID")
        {
            order.OrderStatus = "PENDING";
        }

        if (normalizedStatus == "UNPAID" && latestProof != null && latestProof.VerificationStatus == "APPROVED")
        {
            latestProof.VerificationStatus = "PENDING";
            latestProof.UpdatedAt = DateTime.Now;
        }

        order.UpdatedAt = DateTime.Now;
        _db.SaveChanges();

        return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
    }

    [HttpPost]
    [Authorize(Roles = "MANAGER,ADMIN,STAFF")]
    public IActionResult ReplyOrder(ulong orderId, string staffReply, string? timeFilter)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null || string.IsNullOrWhiteSpace(staffReply))
        {
            return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
        }

        var repliedByClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var repliedBy = ulong.TryParse(repliedByClaim, out var userId) ? userId : (ulong?)null;

        var reply = new OrderReply
        {
            OrderId = order.OrderId,
            RepliedBy = repliedBy,
            ReplyMessage = staffReply.Trim(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.OrderReplies.Add(reply);
        order.UpdatedAt = DateTime.Now;
        _db.SaveChanges();

        return RedirectToAction("OrderMenu", "BackOffice", new { timeFilter });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private ulong? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return ulong.TryParse(claim, out var userId) ? userId : null;
    }

    private string BuildNextPromotionCode()
    {
        var promoCodePattern = new Regex("^PR\\d{3}$", RegexOptions.Compiled);
        var nextPromoNumber = _db.Promotions
            .Select(item => item.PromoCode)
            .AsEnumerable()
            .Where(code => !string.IsNullOrWhiteSpace(code) && promoCodePattern.IsMatch(code))
            .Select(code => int.Parse(code![2..]))
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"PR{nextPromoNumber:D3}";
    }

    private static string BuildCustomerPromotionCode(Promotion promo)
    {
        var baseText = $"{promo.PromotionId}|{promo.PromoCode}|{promo.StartAt:yyyyMMdd}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(baseText));
        var token = Convert.ToHexString(hash)[..8];
        return $"BQ-{token}";
    }

    private static DateTime NormalizePromotionDate(DateTime value)
    {
        return value.Year > 2400 ? value.AddYears(-543) : value;
    }

    private static string NormalizePromoType(string? promoType)
    {
        var normalized = (promoType ?? string.Empty).Trim().ToUpperInvariant();
        var allowedTypes = new HashSet<string> { "PERCENT", "FIXED", "BUY_X_GET_Y", "MEMBER" };
        return allowedTypes.Contains(normalized) ? normalized : string.Empty;
    }

    private bool IsValidPromotionRule(PromotionCreateViewModel input, string promoType)
    {
        return IsValidPromotionRuleInternal(
            promoType,
            input.DiscountPercent,
            input.DiscountAmount,
            input.BuyQty,
            input.FreeQty,
            input.FreeProductId,
            input.MaxRedemptions,
            input.MaxRedemptionsPerUser);
    }

    private bool IsValidPromotionRule(PromotionEditViewModel input, string promoType)
    {
        return IsValidPromotionRuleInternal(
            promoType,
            input.DiscountPercent,
            input.DiscountAmount,
            input.BuyQty,
            input.FreeQty,
            input.FreeProductId,
            input.MaxRedemptions,
            input.MaxRedemptionsPerUser);
    }

    private bool IsValidPromotionRuleInternal(
        string promoType,
        decimal? discountPercent,
        decimal? discountAmount,
        int? buyQty,
        int? freeQty,
        ulong? freeProductId,
        int? maxRedemptions,
        int? maxRedemptionsPerUser)
    {
        if (maxRedemptions.HasValue && maxRedemptions.Value < 0)
        {
            return false;
        }

        if (maxRedemptionsPerUser.HasValue && maxRedemptionsPerUser.Value < 0)
        {
            return false;
        }

        if (promoType == "PERCENT")
        {
            if (!discountPercent.HasValue || discountPercent.Value <= 0 || discountPercent.Value > 100)
            {
                return false;
            }
        }

        if (promoType == "FIXED")
        {
            if (!discountAmount.HasValue || discountAmount.Value <= 0)
            {
                return false;
            }
        }

        if (promoType == "BUY_X_GET_Y")
        {
            if (!buyQty.HasValue || buyQty.Value <= 0 || !freeQty.HasValue || freeQty.Value <= 0 || !freeProductId.HasValue)
            {
                return false;
            }

            if (!_db.Products.Any(p => p.ProductId == freeProductId.Value))
            {
                return false;
            }
        }

        return true;
    }

    private void UpsertPromotionRule(ulong promotionId, PromotionCreateViewModel input, DateTime now)
    {
        var rule = _db.PromotionRules.FirstOrDefault(r => r.PromotionId == promotionId);
        if (rule == null)
        {
            rule = new PromotionRule
            {
                PromotionId = promotionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.PromotionRules.Add(rule);
        }

        rule.MinOrderAmount = input.MinOrderAmount;
        rule.DiscountPercent = input.DiscountPercent;
        rule.DiscountAmount = input.DiscountAmount;
        rule.BuyQty = input.BuyQty;
        rule.FreeQty = input.FreeQty;
        rule.FreeProductId = input.FreeProductId;
        rule.MemberOnly = input.MemberOnly;
        rule.MaxRedemptions = NormalizeLimitValue(input.MaxRedemptions);
        rule.MaxRedemptionsPerUser = NormalizeLimitValue(input.MaxRedemptionsPerUser);
        rule.UpdatedAt = now;
    }

    private void UpsertPromotionRule(ulong promotionId, PromotionEditViewModel input, DateTime now)
    {
        var rule = _db.PromotionRules.FirstOrDefault(r => r.PromotionId == promotionId);
        if (rule == null)
        {
            rule = new PromotionRule
            {
                PromotionId = promotionId,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.PromotionRules.Add(rule);
        }

        rule.MinOrderAmount = input.MinOrderAmount;
        rule.DiscountPercent = input.DiscountPercent;
        rule.DiscountAmount = input.DiscountAmount;
        rule.BuyQty = input.BuyQty;
        rule.FreeQty = input.FreeQty;
        rule.FreeProductId = input.FreeProductId;
        rule.MemberOnly = input.MemberOnly;
        rule.MaxRedemptions = NormalizeLimitValue(input.MaxRedemptions);
        rule.MaxRedemptionsPerUser = NormalizeLimitValue(input.MaxRedemptionsPerUser);
        rule.UpdatedAt = now;
    }

    private static int? NormalizeLimitValue(int? raw)
    {
        if (!raw.HasValue || raw.Value <= 0)
        {
            return null;
        }

        return raw.Value;
    }

    private static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return true;
        }

        return Regex.IsMatch(phone.Trim(), "^\\d{1,10}$");
    }
}
