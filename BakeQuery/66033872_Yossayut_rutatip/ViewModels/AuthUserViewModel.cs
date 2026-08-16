namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class AuthUserViewModel
    {
        public ulong UserId { get; set; }
        public string? UserCode { get; set; }
        public byte RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Status { get; set; }
        public bool RememberMe { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}