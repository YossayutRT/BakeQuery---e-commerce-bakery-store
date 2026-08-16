using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Product
{
    public ulong ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public ushort CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQty { get; set; }

    public string? ImageUrl { get; set; }

    public string Status { get; set; } = null!;

    public ulong? CreatedBy { get; set; }

    public ulong? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PromotionRule> PromotionRules { get; set; } = new List<PromotionRule>();

    public virtual User? UpdatedByNavigation { get; set; }
}
