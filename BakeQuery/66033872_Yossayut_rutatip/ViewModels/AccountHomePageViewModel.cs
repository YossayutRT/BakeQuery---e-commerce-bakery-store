using _66033872_Yossayut_rutatip.Models.db;

namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class AccountHomePageViewModel
    {
        public List<Product> ProductList { get; set; } = new List<Product>();
        public List<Promotion> PromotionList { get; set; } = new List<Promotion>();
        public List<AccountHomeTopSellerViewModel> TopSellingProducts { get; set; } = new List<AccountHomeTopSellerViewModel>();
    }

    public class AccountHomeTopSellerViewModel
    {
        public ulong ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}