using System;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class OrderReply
{
    public ulong ReplyId { get; set; }

    public ulong OrderId { get; set; }

    public ulong? RepliedBy { get; set; }

    public string ReplyMessage { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? RepliedByNavigation { get; set; }
}
