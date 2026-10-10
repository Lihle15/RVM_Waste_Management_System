using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMWMApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Collected, Completed, Cancelled

        // Collection Details
        public string CollectionAddress { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
        public string? CollectionNotes { get; set; }

        // Waste Details
        public string WasteType { get; set; } = string.Empty;
        public decimal EstimatedWeight { get; set; }
        public string? SpecialInstructions { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

// Remove the duplicate OrderItem class definition from this file
// It should be defined in its own file or only once