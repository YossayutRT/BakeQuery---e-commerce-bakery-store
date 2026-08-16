using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Promotion
{
    public ulong PromotionId { get; set; }

    public string PromoCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PromoType { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public bool? IsActive { get; set; }

    public ulong? CreatedBy { get; set; }

    public ulong? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PromotionRedemption> PromotionRedemptions { get; set; } = new List<PromotionRedemption>();

    public virtual ICollection<PromotionRule> PromotionRules { get; set; } = new List<PromotionRule>();

    public virtual User? UpdatedByNavigation { get; set; }
}
