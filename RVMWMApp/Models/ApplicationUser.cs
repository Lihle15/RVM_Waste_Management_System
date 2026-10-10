using Microsoft.AspNetCore.Identity;

namespace RVMWMApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Environmental Impact Tracking
        public decimal TotalRecycledWeight { get; set; } = 0;
        public int TotalRecyclingSessions { get; set; } = 0;
        public decimal CarbonFootprintReduced { get; set; } = 0; // in kg CO2
        public decimal TreesSaved { get; set; } = 0;
        public decimal EnergySaved { get; set; } = 0; // in kWh

        public DateTime MemberSince { get; set; } = DateTime.Now;
        public string? ProfilePicture { get; set; }

        // Navigation properties
        public List<Transaction> Transactions { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<Feedback> Feedback { get; set; } = new();
    }
}