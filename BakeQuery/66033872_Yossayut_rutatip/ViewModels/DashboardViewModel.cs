namespace _66033872_Yossayut_rutatip.ViewModels;

public class DashboardViewModel
{
    public decimal SalesToday { get; set; }
    public decimal SalesThisMonth { get; set; }
    public decimal SalesThisYear { get; set; }

    public List<SalesTrendPointViewModel> SalesTrend { get; set; } = new();
    public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new();
    public List<RoleDistributionViewModel> UserRoleDistribution { get; set; } = new();
}

public class SalesTrendPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TopSellingProductViewModel
{
    public ulong ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class RoleDistributionViewModel
{
    public string RoleName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public string ColorHex { get; set; } = "#6c757d";
}
