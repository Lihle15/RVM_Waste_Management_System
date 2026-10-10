namespace RVMWMApp.Models
{
    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<ReverseVendingMachine> RVMs { get; set; } = new();
    }
}