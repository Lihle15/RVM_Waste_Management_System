using System.ComponentModel.DataAnnotations;

namespace RVMWMApp.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalRVMs { get; set; }
        public int ActiveRVMs { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<string> RecentActivity { get; set; } = new();
    }

    public class UserWithRolesViewModel
    {
        public ApplicationUser? User { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class SystemInfoViewModel
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public DateTime ServerTime => DateTime.Now;
    }
}