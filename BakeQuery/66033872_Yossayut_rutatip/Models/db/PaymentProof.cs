using System;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class PaymentProof
{
    public ulong ProofId { get; set; }

    public ulong OrderId { get; set; }

    public ulong UploadedBy { get; set; }

    public string FilePath { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string VerificationStatus { get; set; } = null!;

    public string? UploadNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User UploadedByNavigation { get; set; } = null!;
}
