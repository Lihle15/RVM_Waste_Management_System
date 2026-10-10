using System.ComponentModel.DataAnnotations;

namespace RVMWMApp.Models
{
    public class WasteItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string WasteType { get; set; } = string.Empty;

        public decimal Weight { get; set; }
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
        public int RvmId { get; set; }
        public ReverseVendingMachine? Rvm { get; set; }
    }
}