using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVMWMApp.Models
{
    public class ReverseVendingMachine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Machine Name")]
        public string MachineName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string Status { get; set; } = "Active";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [DisplayFormat(DataFormatString = "{0:F1}")] // Fix decimal display
        public decimal Capacity { get; set; } = 1000.0m; // Default capacity

        [DisplayFormat(DataFormatString = "{0:F1}")] // Fix decimal display
        public decimal CurrentLoad { get; set; } = 0.0m;

        [Display(Name = "Fill Percentage")]
        public decimal FillPercentage => Capacity > 0 ? (CurrentLoad / Capacity) * 100 : 0;

        // Image property
        public string? ImageUrl { get; set; } = "/images/rvm-default.png";

        public DateTime LastMaintenance { get; set; } = DateTime.Now;
        public DateTime NextMaintenance { get; set; } = DateTime.Now.AddMonths(6);

        // Accepted waste types
        public bool AcceptsPlastic { get; set; } = true;
        public bool AcceptsGlass { get; set; } = true;
        public bool AcceptsMetal { get; set; } = true;
        public bool AcceptsPaper { get; set; } = true;
        public bool AcceptsElectronics { get; set; } = false;

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}