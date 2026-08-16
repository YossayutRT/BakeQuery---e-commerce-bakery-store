using _66033872_Yossayut_rutatip.Models.db;

namespace _66033872_Yossayut_rutatip.ViewModels
{
    public class AccountManagementViewModel
    {
        public List<AuthUserViewModel> UserList { get; set; } = new List<AuthUserViewModel>();
        public List<Role> RoleList { get; set; } = new List<Role>();
        public int ManagerCount { get; set; }
        public int AdminCount { get; set; }
        public int StaffCount { get; set; }
        public int CustomerCount { get; set; }
        public byte? SelectedRoleId { get; set; }
        public string? Keyword { get; set; }
        public string SortBy { get; set; } = "newest";
        public AuthUserViewModel AccountForm { get; set; } = new AuthUserViewModel();
        public AuthUserViewModel EditAccountForm { get; set; } = new AuthUserViewModel();
    }
}
