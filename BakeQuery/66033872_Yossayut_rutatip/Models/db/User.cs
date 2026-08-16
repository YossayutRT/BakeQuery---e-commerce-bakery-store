using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class User
{
    public ulong UserId { get; set; }

    public string UserCode { get; set; } = null!;

    public byte RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PaymentProof> PaymentProofs { get; set; } = new List<PaymentProof>();

    public virtual ICollection<OrderReply> OrderReplies { get; set; } = new List<OrderReply>();

    public virtual ICollection<Product> ProductCreatedByNavigations { get; set; } = new List<Product>();

    public virtual ICollection<Product> ProductUpdatedByNavigations { get; set; } = new List<Product>();

    public virtual ICollection<Promotion> PromotionCreatedByNavigations { get; set; } = new List<Promotion>();

    public virtual ICollection<PromotionRedemption> PromotionRedemptions { get; set; } = new List<PromotionRedemption>();

    public virtual ICollection<Promotion> PromotionUpdatedByNavigations { get; set; } = new List<Promotion>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
}
