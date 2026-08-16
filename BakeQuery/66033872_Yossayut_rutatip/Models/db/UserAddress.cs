using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class UserAddress
{
    public ulong AddressId { get; set; }

    public ulong UserId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Line1 { get; set; } = null!;

    public string? Line2 { get; set; }

    public string? District { get; set; }

    public string Province { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
