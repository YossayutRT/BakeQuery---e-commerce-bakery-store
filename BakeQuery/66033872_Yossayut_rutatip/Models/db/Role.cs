using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Role
{
    public byte RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
