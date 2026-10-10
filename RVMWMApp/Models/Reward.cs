using System.ComponentModel.DataAnnotations;

namespace RVMWMApp.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // Points, Badge, Coupon, Voucher

        [Required]
        public string Description { get; set; } = string.Empty;

        public int Points { get; set; }
        public bool IsRedeemed { get; set; } = false;
        public DateTime EarnedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiresAt { get; set; }

        // For vouchers/coupons
        public string? VoucherCode { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
    }

    public class Badge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // FontAwesome class
        public string Color { get; set; } = "primary";

        public int RequiredPoints { get; set; }
        public string? Criteria { get; set; } // e.g., "Recycle 100kg", "10 sessions"
    }
}