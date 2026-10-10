using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RVMWMApp.Models;

namespace RVMWMApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created and migrated
            await db.Database.MigrateAsync();

            // Create roles
            string[] roleNames = { "Admin", "Customer", "Operator" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Created role: {roleName}");
                }
            }

            // Create admin user
            var adminEmail = "admin@rvm.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };

                var result = await userManager.CreateAsync(admin, "Admin@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"Admin user creation failed: {errors}");
                }
            }

            // Seed Products
            if (!db.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Recycled PET Bottles (Grade A)",
                        Description = "High-quality recycled PET bottles suitable for various manufacturing applications.",
                        Price = 2.50m,
                        Stock = 5000,
                        Category = "Recycled Materials",
                        Rating = 4.5,
                        ReviewCount = 180,
                        IsFeatured = true
                    },
                    new Product
                    {
                        Name = "Aluminum Cans (Crushed)",
                        Description = "Clean, crushed aluminum cans ready for recycling and manufacturing.",
                        Price = 18.50m,
                        Stock = 2500,
                        Category = "Recycled Materials",
                        Rating = 4.2,
                        ReviewCount = 89,
                        IsFeatured = true
                    },
                    new Product
                    {
                        Name = "Mixed Plastic Waste (Sorted)",
                        Description = "Pre-sorted mixed plastic waste including HDPE, LDPE, and PP materials.",
                        Price = 1.25m,
                        Stock = 8000,
                        Category = "Recycled Materials",
                        Rating = 4.7,
                        ReviewCount = 203,
                        IsOnSale = true
                    },
                    new Product
                    {
                        Name = "RVM Maintenance Kit",
                        Description = "Complete maintenance kit for RVM machines including tools and replacement parts.",
                        Price = 1200.00m,
                        Stock = 25,
                        Category = "RVM Equipment",
                        Rating = 4.1,
                        ReviewCount = 34,
                        IsFeatured = true
                    },
                    new Product
                    {
                        Name = "Glass Bottles (Sorted by Color)",
                        Description = "Color-sorted glass bottles ready for recycling and remanufacturing.",
                        Price = 0.85m,
                        Stock = 0,
                        Category = "Recycled Materials",
                        Rating = 4.0,
                        ReviewCount = 87
                    }
                };

                db.Products.AddRange(products);
                await db.SaveChangesAsync();
            }

            // Seed RVMs
            if (!db.RVMs.Any())
            {
                var rvms = new List<ReverseVendingMachine>
                {
                    new ReverseVendingMachine { MachineName = "RVM-001", Location = "Durban Beachfront", Capacity = 100, CurrentLoad = 15, IsActive = true },
                    new ReverseVendingMachine { MachineName = "RVM-002", Location = "Umhlanga Rocks",    Capacity = 100, CurrentLoad = 45, IsActive = true },
                    new ReverseVendingMachine { MachineName = "RVM-003", Location = "Gateway Mall",      Capacity = 100, CurrentLoad = 78, IsActive = true },
                    new ReverseVendingMachine { MachineName = "RVM-004", Location = "Scottville",        Capacity = 100, CurrentLoad = 15, IsActive = true },
                    new ReverseVendingMachine { MachineName = "RVM-005", Location = "Liberty Mall",      Capacity = 100, CurrentLoad = 45, IsActive = true },
                    new ReverseVendingMachine { MachineName = "RVM-006", Location = "Edendale Mall",     Capacity = 100, CurrentLoad = 78, IsActive = true }
                };

                db.RVMs.AddRange(rvms);
                await db.SaveChangesAsync();
                Console.WriteLine("Sample RVMs created successfully!");
            }

            // (The sample "Carts" seeding was removed: the cart lives in the session, not the database.)

            Console.WriteLine("Seeding completed.");
        }
    }
}