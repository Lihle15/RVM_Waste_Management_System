using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RVMWMApp.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // ✅ optional image for recyclables
        public string? ImageUrl { get; set; }

        // ✅ calculated field (not stored in DB)
        [NotMapped]
        public decimal Total => Price * Quantity;
    }
}
