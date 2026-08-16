using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Category
{
    public ushort CategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
