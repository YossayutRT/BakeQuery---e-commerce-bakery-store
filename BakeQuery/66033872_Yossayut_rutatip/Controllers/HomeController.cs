using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _66033872_Yossayut_rutatip.Models;
using _66033872_Yossayut_rutatip.Models.db;
using _66033872_Yossayut_rutatip.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Xml;
namespace _66033872_Yossayut_rutatip.Controllers;

public class HomeController : Controller
{

    private readonly Csi402BakequeryContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(Csi402BakequeryContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }
    public IActionResult Index(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            ViewBag.WelcomeMessage = $"Welcome {email}";
        }

        // int No1,No2,No3,No4;
        // No1 = 1;
        // No2 = 2;
        // No3 = 3;
        // No4 = No1 + No2 + No3;
        // char T1 = 'A',T2 = 'B';
        // string tx1;
        // tx1 = Convert.ToString(T1) + Convert.ToString(T2);
        // ViewBag.R1 = tx1;

        // Assesment 1 66033872
        string fullName = "ยศยุต ฤธาทิพย์";
        int studentId = 66033872;
        string section = "CSI402 L001";
        string Years = "ชั้นปีที่ 3";
        string Language = "Javascript HTML CSS";
        ViewBag.fullName = fullName;
        ViewBag.studentId = studentId;
        ViewBag.section = section;
        ViewBag.Years = Years;
        ViewBag.Language = Language;

        // เก็บตัวแปรเก็บคะแนนครั้งที่ 1-10 
        int score1, score2, score3, score4, score5, score6, score7, score8, score9, score10;
        score1 = 10;
        score2 = 10;
        score3 = 10;
        score4 = 10;
        score5 = 10;
        score6 = 10;
        score7 = 10;
        score8 = 10;
        score9 = 10;
        score10 = 10;
        ViewBag.score1 = score1;
        ViewBag.score2 = score2;
        ViewBag.score3 = score3;
        ViewBag.score4 = score4;
        ViewBag.score5 = score5;
        ViewBag.score6 = score6;
        ViewBag.score7 = score7;
        ViewBag.score8 = score8;
        ViewBag.score9 = score9;
        ViewBag.score10 = score10;

        int sum = score1 + score2 + score3 + score4 + score5 + score6 + score7 + score8 + score9 + score10;
        ViewBag.sum = sum;
        
        if (sum >= 80)
        {
            ViewBag.grade = "A";
        }
        else if (sum >= 76 && sum <= 79)
        {
            ViewBag.grade = "B+";
        }
        else if (sum >= 70 && sum <= 75)
        {
            ViewBag.grade = "B";
        }
        else if (sum >= 66 && sum <= 69)
        {
            ViewBag.grade = "C+";
        }
        else if (sum >= 60 && sum <= 65)
        {
            ViewBag.grade = "C";
        }
        else if (sum >= 56 && sum <= 59)
        {
            ViewBag.grade = "D+";
        }
        else if (sum >= 50 && sum <= 55)
        {
            ViewBag.grade = "D";
        }
        else
        {
            ViewBag.grade = "F";
        }

        var products = _db.Products
            .Include(p => p.Category)
            .Where(p => p.Status != "INACTIVE" && p.StockQty > 0)
            .OrderByDescending(p => p.ProductId)
            .ToList();

        var topSellingProducts = _db.OrderItems
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
            .Take(4)
            .ToList();

        var model = new HomeIndexViewModel
        {
            Products = products,
            TopSellingProducts = topSellingProducts
        };

        return View(model);
    }

    public IActionResult Lab9List()
    {
        var user = (from u in _db.LabStudents select u).ToList();
        return View(user);
    }

    public IActionResult Lab10(string UID)
    {
        var check = ( from us in _db.LabStudents where us.StdID == UID select new Lab9UserViewModel
        {
            UserId = us.StdID,
            Password = us.StdPASSWORD,
            Name = us.StdName,
            Lastname = us.StdLastname
        }).FirstOrDefault();
        
        return View(check);
    }

    [HttpPost]
    public IActionResult Lab10(Lab9UserViewModel data)
    {
        var user = (from u in _db.LabStudents where u.StdID == data.UserId select u).FirstOrDefault();

        if (user == null)
        {
            return NotFound();
        }

        user.StdName = data.Name;
        user.StdLastname = data.Lastname;
        user.StdPASSWORD = data.Password;

        _db.Update(user);
        _db.SaveChanges();
        return RedirectToAction("Lab9List","Home");
    }
    public IActionResult Lab10D(string UID)
    {
        var user = (from u in _db.LabStudents where u.StdID == UID select u).FirstOrDefault();

        _db.RemoveRange(user);
        _db.SaveChanges();
        return RedirectToAction("Lab9List","Home");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
