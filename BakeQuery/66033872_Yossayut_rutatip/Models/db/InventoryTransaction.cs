using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class InventoryTransaction
{
    public ulong TransactionId { get; set; }

    public ulong ProductId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int QtyChange { get; set; }

    public string ReferenceType { get; set; } = null!;

    public ulong? ReferenceId { get; set; }

    public string? Note { get; set; }

    public ulong? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;
}
