namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class OrderManagementViewModel
    {
        public List<OrderManagementItemViewModel> Orders { get; set; } = new List<OrderManagementItemViewModel>();
        public string SelectedTimeFilter { get; set; } = "day";

        public List<string> OrderStatuses { get; set; } = new List<string>
        {
            "PENDING",
            "PAID",
            "PREPARING",
            "SHIPPING",
            "DELIVERED",
            "CANCELLED"
        };

        public List<string> PaymentStatuses { get; set; } = new List<string>
        {
            "UNPAID",
            "PAID",
            "REFUNDED"
        };
    }

    public class OrderManagementItemViewModel
    {
        public ulong OrderId { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerNote { get; set; }
        public List<string> ItemNames { get; set; } = new List<string>();
        public List<OrderReplyItemViewModel> Replies { get; set; } = new List<OrderReplyItemViewModel>();
        public OrderPaymentProofViewModel? LatestPaymentProof { get; set; }
    }

    public class OrderReplyItemViewModel
    {
        public string RepliedByName { get; set; } = string.Empty;
        public string ReplyMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class OrderPaymentProofViewModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public string? UploadNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
