using System.ComponentModel.DataAnnotations;

namespace RVMWMApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public int Stock { get; set; }
        public string? Category { get; set; }
        public string? ImagePath { get; set; }
        public double Rating { get; set; } = 4.0;
        public int ReviewCount { get; set; } = 0;
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        public string? ImageUrl { get; set; }
        //public string? Tags { get; set; } // For search functionality
    }
}
