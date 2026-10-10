using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMWMApp.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // Recycling, Purchase, Reward, etc.

        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // For recycling transactions
        public string? WasteType { get; set; }
        public decimal? Weight { get; set; }
        public int? ReverseVendingMachineId { get; set; }

        [ForeignKey("ReverseVendingMachineId")]
        public ReverseVendingMachine? ReverseVendingMachine { get; set; }

        public string Status { get; set; } = "Completed";
    }
}