using System.ComponentModel.DataAnnotations;

namespace RVMWMApp.Models
{
    public class IllegalDumpingReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Landmark { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? WasteType { get; set; }
        public string? EstimatedQuantity { get; set; }

        public string? PhotoUrl { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Reported"; // Reported, Under Review, Resolved, Closed
        public string? AdminNotes { get; set; }
    }
}