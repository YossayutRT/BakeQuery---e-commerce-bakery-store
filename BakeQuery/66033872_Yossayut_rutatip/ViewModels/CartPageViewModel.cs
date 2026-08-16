namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class CartPageViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public int TotalQty { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string? CheckoutNote { get; set; }
        public string? PromoCode { get; set; }
        public string? PromoMessage { get; set; }
        public bool IsPromoApplied { get; set; }
        public bool HasDefaultAddress { get; set; }
        public string? DefaultAddressDisplay { get; set; }
    }

    public class CartItemViewModel
    {
        public ulong CartItemId { get; set; }
        public ulong ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Qty { get; set; }
        public int StockQty { get; set; }
        public decimal LineTotal => UnitPrice * Qty;
    }
}
