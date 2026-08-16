namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class ForgotPasswordViewModel
    {
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? OtpCode { get; set; }
        public string? MockOtp { get; set; }
        public bool IsOtpStage { get; set; }
    }
}
