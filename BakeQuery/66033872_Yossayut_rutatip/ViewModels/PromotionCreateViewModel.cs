using _66033872_Yossayut_rutatip.Models.db;

namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class PromotionCreateViewModel
    {
        public string? PromoCode { get; set; }
        public string? Name { get; set; }
        public string? PromoType { get; set; }
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsActive { get; set; } = true;

        public decimal? MinOrderAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? BuyQty { get; set; }
        public int? FreeQty { get; set; }
        public ulong? FreeProductId { get; set; }
        public bool MemberOnly { get; set; }
        public int? MaxRedemptions { get; set; }
        public int? MaxRedemptionsPerUser { get; set; }
    }

    public class PromotionsPageViewModel
    {
        public List<PromotionListItemViewModel> PromotionList { get; set; } = new List<PromotionListItemViewModel>();
        public List<ProductOptionViewModel> ProductOptions { get; set; } = new List<ProductOptionViewModel>();
        public PromotionCreateViewModel PromotionForm { get; set; } = new PromotionCreateViewModel();
        public PromotionEditViewModel EditPromotionForm { get; set; } = new PromotionEditViewModel();
    }

    public class PromotionEditViewModel
    {
        public ulong PromotionId { get; set; }
        public string? PromoCode { get; set; }
        public string? Name { get; set; }
        public string? PromoType { get; set; }
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsActive { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? BuyQty { get; set; }
        public int? FreeQty { get; set; }
        public ulong? FreeProductId { get; set; }
        public bool MemberOnly { get; set; }
        public int? MaxRedemptions { get; set; }
        public int? MaxRedemptionsPerUser { get; set; }
    }

    public class PromotionListItemViewModel
    {
        public ulong PromotionId { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PromoType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsActive { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? BuyQty { get; set; }
        public int? FreeQty { get; set; }
        public ulong? FreeProductId { get; set; }
        public string? FreeProductName { get; set; }
        public bool MemberOnly { get; set; }
        public int? MaxRedemptions { get; set; }
        public int? MaxRedemptionsPerUser { get; set; }
        public int RedemptionCount { get; set; }
    }

    public class ProductOptionViewModel
    {
        public ulong ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}