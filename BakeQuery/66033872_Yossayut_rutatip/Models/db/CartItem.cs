using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class CartItem
{
    public ulong CartItemId { get; set; }

    public ulong CartId { get; set; }

    public ulong ProductId { get; set; }

    public int Qty { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? LineTotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
