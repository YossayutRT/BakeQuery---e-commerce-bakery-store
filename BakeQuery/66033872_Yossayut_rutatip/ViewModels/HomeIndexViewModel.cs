using _66033872_Yossayut_rutatip.Models.db;

namespace _66033872_Yossayut_rutatip.ViewModels;

public class HomeIndexViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new();
}
