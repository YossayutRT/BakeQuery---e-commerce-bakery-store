using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class PromotionRule
{
    public ulong RuleId { get; set; }

    public ulong PromotionId { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public int? BuyQty { get; set; }

    public int? FreeQty { get; set; }

    public ulong? FreeProductId { get; set; }

    public bool MemberOnly { get; set; }

    public int? MaxRedemptions { get; set; }

    public int? MaxRedemptionsPerUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product? FreeProduct { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;
}
