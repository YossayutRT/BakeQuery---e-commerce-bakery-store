using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _66033872_Yossayut_rutatip.Models;
using _66033872_Yossayut_rutatip.Models.db;
using _66033872_Yossayut_rutatip.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace _66033872_Yossayut_rutatip.Controllers;

public class AccountController : Controller
{
    private const string ForgotOtpSessionKey = "forgot_password_otp";
    private const string ForgotOtpEmailSessionKey = "forgot_password_email";
    private const string ForgotOtpExpireSessionKey = "forgot_password_expire";

    // Database
    private readonly Csi402BakequeryContext _db;
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;

    public AccountController(Csi402BakequeryContext db, ILogger<AccountController> logger, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    [Authorize]
    public IActionResult Profile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.UserId == userId.Value);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var address = _db.UserAddresses
            .AsNoTracking()
            .Where(a => a.UserId == userId.Value)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.UpdatedAt)
            .FirstOrDefault();

        var vm = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            RecipientName = address?.RecipientName ?? user.FullName,
            AddressPhone = address?.Phone ?? (user.Phone ?? string.Empty),
            Line1 = address?.Line1 ?? string.Empty,
            Line2 = address?.Line2,
            District = address?.District,
            Province = address?.Province ?? string.Empty,
            PostalCode = address?.PostalCode ?? string.Empty,
            Country = address?.Country ?? "Thailand",
            HasDefaultAddress = address != null,
            Message = TempData["ProfileMessage"]?.ToString(),
            MessageType = TempData["ProfileMessageType"]?.ToString()
        };

        return View(vm);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Profile(ProfileViewModel data)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users.FirstOrDefault(u => u.UserId == userId.Value);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(data.FullName) || string.IsNullOrWhiteSpace(data.Email))
        {
            TempData["ProfileMessageType"] = "danger";
            TempData["ProfileMessage"] = "กรุณากรอกชื่อและอีเมลให้ครบ";
            return RedirectToAction("Profile", "Account");
        }

        if (!IsValidPhone(data.Phone) || !IsValidPhone(data.AddressPhone))
        {
            TempData["ProfileMessageType"] = "danger";
            TempData["ProfileMessage"] = "เบอร์โทรต้องเป็นตัวเลขเท่านั้น และไม่เกิน 10 หลัก";
            return RedirectToAction("Profile", "Account");
        }

        var phone = string.IsNullOrWhiteSpace(data.Phone) ? null : data.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var isDuplicatePhone = _db.Users.Any(u => u.UserId != userId.Value && u.Phone != null && u.Phone.Trim() == phone);
            if (isDuplicatePhone)
            {
                TempData["ProfileMessageType"] = "danger";
                TempData["ProfileMessage"] = "เบอร์โทรนี้ถูกใช้งานแล้ว";
                return RedirectToAction("Profile", "Account");
            }
        }

        if (string.IsNullOrWhiteSpace(data.Line1) ||
            string.IsNullOrWhiteSpace(data.Province) ||
            string.IsNullOrWhiteSpace(data.PostalCode) ||
            string.IsNullOrWhiteSpace(data.RecipientName) ||
            string.IsNullOrWhiteSpace(data.AddressPhone))
        {
            TempData["ProfileMessageType"] = "danger";
            TempData["ProfileMessage"] = "กรุณากรอกข้อมูลที่อยู่จัดส่งให้ครบถ้วน";
            return RedirectToAction("Profile", "Account");
        }

        if (!Regex.IsMatch(data.PostalCode.Trim(), "^\\d{5}$"))
        {
            TempData["ProfileMessageType"] = "danger";
            TempData["ProfileMessage"] = "รหัสไปรษณีย์ต้องเป็นตัวเลข 5 หลัก";
            return RedirectToAction("Profile", "Account");
        }

        var email = data.Email.Trim();
        var isDuplicateEmail = _db.Users.Any(u => u.UserId != userId.Value && u.Email.ToLower() == email.ToLower());
        if (isDuplicateEmail)
        {
            TempData["ProfileMessageType"] = "danger";
            TempData["ProfileMessage"] = "อีเมลนี้ถูกใช้งานแล้ว";
            return RedirectToAction("Profile", "Account");
        }

        user.FullName = data.FullName.Trim();
        user.Email = email;
        user.Phone = phone;
        user.UpdatedAt = DateTime.Now;

        var address = _db.UserAddresses
            .Where(a => a.UserId == userId.Value)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.UpdatedAt)
            .FirstOrDefault();

        if (address == null)
        {
            address = new UserAddress
            {
                UserId = userId.Value,
                CreatedAt = DateTime.Now
            };

            _db.UserAddresses.Add(address);
        }

        address.RecipientName = data.RecipientName.Trim();
        address.Phone = data.AddressPhone.Trim();
        address.Line1 = data.Line1.Trim();
        address.Line2 = string.IsNullOrWhiteSpace(data.Line2) ? null : data.Line2.Trim();
        address.District = string.IsNullOrWhiteSpace(data.District) ? null : data.District.Trim();
        address.Province = data.Province.Trim();
        address.PostalCode = data.PostalCode.Trim();
        address.Country = string.IsNullOrWhiteSpace(data.Country) ? "Thailand" : data.Country.Trim();
        address.IsDefault = true;
        address.UpdatedAt = DateTime.Now;

        _db.SaveChanges();

        TempData["ProfileMessageType"] = "success";
        TempData["ProfileMessage"] = "บันทึกข้อมูลโปรไฟล์และที่อยู่เรียบร้อยแล้ว";
        return RedirectToAction("Profile", "Account");
    }

    [AllowAnonymous]
    public IActionResult HomePage(string Email, string Password)
    {
        ViewBag.Email = Email;
        ViewBag.Password = Password;

        var data = new AccountHomePageViewModel();

        data.ProductList = _db.Products
            .Include(p => p.Category)
            .Where(p => p.Status != "INACTIVE" && p.StockQty > 0)
            .OrderByDescending(p => p.ProductId)
            .ToList();

        data.PromotionList = _db.Promotions
            .Where(p => p.IsActive == true)
            .OrderByDescending(p => p.PromotionId)
            .Take(5)
            .ToList();

        data.TopSellingProducts = _db.OrderItems
            .AsNoTracking()
            .Where(oi =>
                oi.Order.OrderStatus.ToUpper() != "PENDING" &&
                oi.Order.OrderStatus.ToUpper() != "CANCELLED")
            .Where(oi =>
                oi.Order.PaymentStatus.ToUpper() == "PAID" ||
                oi.Order.PaymentProofs.Any(p => p.VerificationStatus.ToUpper() == "APPROVED"))
            .GroupBy(oi => new
            {
                oi.ProductId,
                oi.Product.Name,
                CategoryName = oi.Product.Category.Name,
                oi.Product.Price,
                oi.Product.ImageUrl
            })
            .Select(g => new AccountHomeTopSellerViewModel
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                Price = g.Key.Price,
                ImageUrl = g.Key.ImageUrl,
                QuantitySold = g.Sum(x => x.Qty),
                Revenue = g.Sum(x => (decimal?)((x.LineTotal ?? 0m) > 0m
                    ? x.LineTotal
                    : x.UnitPrice * x.Qty)) ?? 0m
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        return View("~/Views/Account/HomePage.cshtml", data);
    }
    public IActionResult Lab5()
    {
        // var user = new List<LabUserViewModel>
        // {
        //     new LabUserViewModel{UserId = "Aaa" , Name = "Yossayut" , Lastname = "Rutatip"},
        //     new LabUserViewModel{UserId = "Bbb" , Name = "Thiraphat" , Lastname = "Sangsorn"}
        // };
        // return View(user);
        return View();
    }


    [HttpPost]
    public IActionResult Lab5(LabUserViewModel data)
    {
        ViewBag.UserId = data.UserId ?? string.Empty;
        ViewBag.Name = data.Name ?? string.Empty;
        ViewBag.Lastname = data.Lastname ?? string.Empty;

        // return View();
        return RedirectToAction("Lab52","Account", new { UserId = data.UserId, Name = data.Name, Lastname = data.Lastname });
    }

    public IActionResult Lab52(string UserId, string Name, string Lastname)
    {
        ViewBag.UserId = UserId;
        ViewBag.Name = Name;
        ViewBag.Lastname = Lastname;
        return View();
    }


    [AllowAnonymous]
    public IActionResult Login()
    {
        return View("~/Views/LoginRegister/Login.cshtml", new AuthUserViewModel());
    }

    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View("~/Views/LoginRegister/ForgotPassword.cshtml", new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult RequestForgotPasswordOtp(ForgotPasswordViewModel data)
    {
        if (string.IsNullOrWhiteSpace(data.Email) ||
            string.IsNullOrWhiteSpace(data.NewPassword) ||
            string.IsNullOrWhiteSpace(data.ConfirmPassword))
        {
            return Json(new { success = false, message = "กรุณากรอกอีเมลและรหัสผ่านใหม่ให้ครบก่อนขอ OTP" });
        }

        if (data.NewPassword.Trim().Length < 6)
        {
            return Json(new { success = false, message = "รหัสผ่านใหม่ต้องมีอย่างน้อย 6 ตัวอักษร" });
        }

        if (!string.Equals(data.NewPassword, data.ConfirmPassword, StringComparison.Ordinal))
        {
            return Json(new { success = false, message = "ยืนยันรหัสผ่านไม่ตรงกัน" });
        }

        var email = data.Email.Trim();
        var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
        {
            return Json(new { success = false, message = "ไม่พบบัญชีผู้ใช้งานของอีเมลนี้" });
        }

        var generatedOtp = GenerateMockOtp();
        HttpContext.Session.SetString(ForgotOtpSessionKey, generatedOtp);
        HttpContext.Session.SetString(ForgotOtpEmailSessionKey, email.ToLowerInvariant());
        HttpContext.Session.SetString(ForgotOtpExpireSessionKey, DateTime.UtcNow.AddMinutes(5).ToString("o"));

        return Json(new
        {
            success = true,
            message = "สร้าง OTP สําเร็จแล้ว กรุณากรอกรหัส OTP เพื่อยืนยัน",
            mockOtp = generatedOtp
        });
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult ForgotPassword(ForgotPasswordViewModel data)
    {
        if (string.IsNullOrWhiteSpace(data.Email) ||
            string.IsNullOrWhiteSpace(data.NewPassword) ||
            string.IsNullOrWhiteSpace(data.ConfirmPassword))
        {
            ModelState.AddModelError(string.Empty, "กรุณากรอกข้อมูลให้ครบถ้วน");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        if (data.NewPassword.Trim().Length < 6)
        {
            ModelState.AddModelError(string.Empty, "รหัสผ่านใหม่ต้องมีอย่างน้อย 6 ตัวอักษร");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        if (!string.Equals(data.NewPassword, data.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "ยืนยันรหัสผ่านไม่ตรงกัน");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        var email = data.Email.Trim();
        var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "ไม่พบบัญชีผู้ใช้งานของอีเมลนี้");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        var normalizedEmail = email.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(data.OtpCode))
        {
            ModelState.AddModelError(string.Empty, "กรุณากดขอ OTP และกรอกรหัส OTP ก่อนเปลี่ยนรหัสผ่าน");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        var expectedOtp = HttpContext.Session.GetString(ForgotOtpSessionKey);
        var expectedEmail = HttpContext.Session.GetString(ForgotOtpEmailSessionKey);
        var expireAtRaw = HttpContext.Session.GetString(ForgotOtpExpireSessionKey);

        if (string.IsNullOrWhiteSpace(expectedOtp) || string.IsNullOrWhiteSpace(expectedEmail) || string.IsNullOrWhiteSpace(expireAtRaw))
        {
            ModelState.AddModelError(string.Empty, "OTP หมดอายุหรือยังไม่ได้สร้าง กรุณาขอ OTP ใหม่");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        if (!DateTime.TryParse(expireAtRaw, out var expireAt) || DateTime.UtcNow > expireAt)
        {
            ClearForgotOtpSession();
            ModelState.AddModelError(string.Empty, "OTP หมดอายุแล้ว กรุณาขอ OTP ใหม่");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        if (!string.Equals(expectedEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            ClearForgotOtpSession();
            ModelState.AddModelError(string.Empty, "อีเมลไม่ตรงกับคำขอ OTP ล่าสุด กรุณาขอ OTP ใหม่");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        if (!string.Equals(data.OtpCode.Trim(), expectedOtp, StringComparison.Ordinal))
        {
            data.IsOtpStage = true;
            data.MockOtp = expectedOtp;
            ModelState.AddModelError(string.Empty, "OTP ไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง");
            return View("~/Views/LoginRegister/ForgotPassword.cshtml", data);
        }

        user.PasswordHash = data.NewPassword;
        user.UpdatedAt = DateTime.Now;
        _db.SaveChanges();
        ClearForgotOtpSession();

        TempData["AuthSuccess"] = "รีเซ็ตรหัสผ่านเรียบร้อยแล้ว กรุณาเข้าสู่ระบบ";
        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(AuthUserViewModel data)
    {
        if (string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Password))
        {
            ModelState.AddModelError(string.Empty, "กรุณากรอกอีเมลและรหัสผ่าน");
            return View("~/Views/LoginRegister/Login.cshtml", data);
        }

        var email = data.Email.Trim();
        var user = _db.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.PasswordHash == data.Password);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "อีเมลหรือรหัสผ่านไม่ถูกต้อง");
            return View("~/Views/LoginRegister/Login.cshtml", data);
        }

        if (!string.Equals(user.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "บัญชีถูกปิดการใช้งาน กรุณาติดต่อผู้ดูแลระบบ");
            return View("~/Views/LoginRegister/Login.cshtml", data);
        }

        var roleName = (user.Role?.RoleName ?? "CUSTOMER").Trim().ToUpperInvariant();
        var token = CreateJwtToken(user, roleName, false);
        SetAuthCookie(token, false);

        user.LastLoginAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;
        _db.SaveChanges();

        return roleName switch
        {
            "MANAGER" => RedirectToAction("AdminPage", "BackOffice"),
            "ADMIN" => RedirectToAction("OrderMenu", "BackOffice"),
            "STAFF" => RedirectToAction("OrderMenu", "BackOffice"),
            _ => RedirectToAction("HomePage", "Account")
        };
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View("~/Views/LoginRegister/Register.cshtml", new AuthUserViewModel());
    }
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Register(AuthUserViewModel data)
    {
        if (string.IsNullOrWhiteSpace(data.FullName) ||
            string.IsNullOrWhiteSpace(data.Email) ||
            string.IsNullOrWhiteSpace(data.Password) ||
            string.IsNullOrWhiteSpace(data.ConfirmPassword))
        {
            ModelState.AddModelError(string.Empty, "กรุณากรอกข้อมูลให้ครบถ้วน");
            return View("~/Views/LoginRegister/Register.cshtml", data);
        }

        if (!string.Equals(data.Password, data.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "ยืนยันรหัสผ่านไม่ตรงกัน");
            return View("~/Views/LoginRegister/Register.cshtml", data);
        }

        if (data.Password.Trim().Length < 6)
        {
            ModelState.AddModelError(string.Empty, "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร");
            return View("~/Views/LoginRegister/Register.cshtml", data);
        }

        var email = data.Email.Trim();
        var isDuplicateEmail = _db.Users.Any(u => u.Email.ToLower() == email.ToLower());
        if (isDuplicateEmail)
        {
            ModelState.AddModelError(string.Empty, "อีเมลนี้ถูกใช้งานแล้ว");
            return View("~/Views/LoginRegister/Register.cshtml", data);
        }

        if (!IsValidPhone(data.Phone))
        {
            ModelState.AddModelError(string.Empty, "เบอร์โทรต้องเป็นตัวเลขเท่านั้น และไม่เกิน 10 หลัก");
            return View("~/Views/LoginRegister/Register.cshtml", data);
        }

        var phone = string.IsNullOrWhiteSpace(data.Phone) ? null : data.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var isDuplicatePhone = _db.Users.Any(u => u.Phone != null && u.Phone.Trim() == phone);
            if (isDuplicatePhone)
            {
                ModelState.AddModelError(string.Empty, "เบอร์โทรนี้ถูกใช้งานแล้ว");
                return View("~/Views/LoginRegister/Register.cshtml", data);
            }
        }

        var customerRole = _db.Roles.FirstOrDefault(r => r.RoleName.ToLower() == "customer");
        if (customerRole == null)
        {
            customerRole = new Role
            {
                RoleName = "CUSTOMER",
                Description = "Customer role",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Roles.Add(customerRole);
            _db.SaveChanges();
        }

        var roleId = customerRole.RoleId;
        var userCodePattern = new Regex("^U\\d+$", RegexOptions.Compiled);
        var nextCodeNumber = _db.Users
            .Select(item => item.UserCode)
            .AsEnumerable()
            .Where(code => !string.IsNullOrWhiteSpace(code) && userCodePattern.IsMatch(code))
            .Select(code => ulong.TryParse(code![1..], out var n) ? n : 0UL)
            .DefaultIfEmpty(0UL)
            .Max() + 1;
        var userCode = $"U{nextCodeNumber:D3}";

        var u = new User();
        u.UserCode = userCode;
        u.RoleId = roleId;
        u.Email = email;
        u.PasswordHash = data.Password;
        u.FullName = data.FullName.Trim();
        u.Phone = phone;
        u.Status = "ACTIVE";
        u.LastLoginAt = DateTime.Now;
        u.CreatedAt = DateTime.Now;
        u.UpdatedAt = DateTime.Now;

        _db.Users.Add(u);
        _db.SaveChanges();

        var token = CreateJwtToken(u, "CUSTOMER", true);
        SetAuthCookie(token, true);

        return RedirectToAction("HomePage", "Account", new { Email = data.Email });
    }

    private static string GenerateMockOtp()
    {
        return Random.Shared.Next(0, 1000000).ToString("D6");
    }

    private void ClearForgotOtpSession()
    {
        HttpContext.Session.Remove(ForgotOtpSessionKey);
        HttpContext.Session.Remove(ForgotOtpEmailSessionKey);
        HttpContext.Session.Remove(ForgotOtpExpireSessionKey);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Logout()
    {
        ClearAuthCookie();
        return RedirectToAction("Login", "Account");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Menu()
    {
        return View("~/Views/PublicPage/Menu.cshtml");
    }

    public IActionResult Promotion()
    {
        return View("~/Views/PublicPage/Promotion.cshtml");
    }

    public IActionResult Lab8()
    {
        var user = (from u in _db.Users select u).ToList();
        return View(user);
    }

    public IActionResult Lab9()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Lab9(Lab9UserViewModel data)
    {
        var u = new LabStudent();
        u.StdID = data.UserId ?? string.Empty;
        u.StdPASSWORD = data.Password ?? string.Empty;
        u.StdName = data.Name ?? string.Empty;
        u.StdLastname = data.Lastname ?? string.Empty;
        _db.LabStudents.Add(u);
        _db.SaveChanges();

        return RedirectToAction("Lab9List", "Home");
    }

   

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private string CreateJwtToken(User user, string roleName, bool rememberMe)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? "BakeQuery";
        var audience = jwtSection["Audience"] ?? "BakeQueryClient";
        var secret = jwtSection["Secret"] ?? "BakeQuery_Default_Secret_Key_Change_Me_123456";
        var expiresAt = DateTime.UtcNow.AddDays(rememberMe ? 14 : 1);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, roleName)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private void SetAuthCookie(string token, bool rememberMe)
    {
        Response.Cookies.Append("bakequery_access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(rememberMe ? 14 : 1)
        });
    }

    private void ClearAuthCookie()
    {
        Response.Cookies.Delete("bakequery_access_token");
    }

    private static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return true;
        }

        return Regex.IsMatch(phone.Trim(), "^\\d{1,10}$");
    }

    private ulong? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return ulong.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
