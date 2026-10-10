using RVMWMApp.Models;

namespace RVMWMApp.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public EnvironmentalImpact Impact { get; set; } = null!;
    }

    public class EnvironmentalImpact
    {
        public decimal RecycledWeight { get; set; }
        public decimal CarbonReduced { get; set; }
        public decimal TreesSaved { get; set; }
        public decimal EnergySaved { get; set; }
    }
}