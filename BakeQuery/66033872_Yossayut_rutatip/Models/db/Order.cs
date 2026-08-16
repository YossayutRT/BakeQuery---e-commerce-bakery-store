using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Order
{
    public ulong OrderId { get; set; }

    public string OrderNo { get; set; } = null!;

    public ulong UserId { get; set; }

    public ulong AddressId { get; set; }

    public ulong? PromotionId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal GrandTotal { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual UserAddress Address { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PaymentProof> PaymentProofs { get; set; } = new List<PaymentProof>();

    public virtual ICollection<OrderReply> OrderReplies { get; set; } = new List<OrderReply>();

    public virtual Promotion? Promotion { get; set; }

    public virtual ICollection<PromotionRedemption> PromotionRedemptions { get; set; } = new List<PromotionRedemption>();

    public virtual User User { get; set; } = null!;
}
