using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Cart
{
    public ulong CartId { get; set; }

    public ulong UserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;
}
