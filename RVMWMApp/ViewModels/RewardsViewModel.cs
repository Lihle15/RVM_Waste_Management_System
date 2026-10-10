using RVMWMApp.Models;

namespace RVMWMApp.ViewModels
{
    public class RewardsViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Reward> Rewards { get; set; } = new();
        public List<Badge> Badges { get; set; } = new();
        public int TotalPoints { get; set; }
    }
}