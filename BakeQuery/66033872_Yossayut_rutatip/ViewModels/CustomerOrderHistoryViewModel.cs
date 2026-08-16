namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class CustomerOrderHistoryViewModel
    {
        public List<CustomerOrderItemViewModel> Orders { get; set; } = new List<CustomerOrderItemViewModel>();
    }

    public class CustomerOrderItemViewModel
    {
        public ulong OrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerNote { get; set; }
        public List<string> Items { get; set; } = new List<string>();
        public List<CustomerOrderReplyViewModel> Replies { get; set; } = new List<CustomerOrderReplyViewModel>();
        public List<CustomerPaymentProofViewModel> PaymentProofs { get; set; } = new List<CustomerPaymentProofViewModel>();
    }

    public class CustomerOrderReplyViewModel
    {
        public string RepliedBy { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerPaymentProofViewModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public string? UploadNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
