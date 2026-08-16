using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class PromotionRedemption
{
    public ulong RedemptionId { get; set; }

    public ulong PromotionId { get; set; }

    public ulong UserId { get; set; }

    public ulong OrderId { get; set; }

    public decimal DiscountValue { get; set; }

    public DateTime RedeemedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
