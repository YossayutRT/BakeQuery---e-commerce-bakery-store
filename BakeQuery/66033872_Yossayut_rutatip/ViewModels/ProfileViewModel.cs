namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }

        public string RecipientName { get; set; } = string.Empty;
        public string AddressPhone { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string? District { get; set; }
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Thailand";

        public bool HasDefaultAddress { get; set; }
        public string? Message { get; set; }
        public string? MessageType { get; set; }
    }
}
