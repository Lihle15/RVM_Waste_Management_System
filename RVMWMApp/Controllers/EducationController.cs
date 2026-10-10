using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace RVMWMApp.Controllers
{
    public class EducationController : Controller
    {
        public IActionResult Index()
        {
            var facts = new List<RecyclingFact>
            {
                new RecyclingFact
                {
                    Title = "Plastic Recycling",
                    Content = "Recycling one plastic bottle saves enough energy to power a 60W light bulb for 6 hours.",
                    ImageUrl = "/images/plastic-recycling.jpg"
                },
                new RecyclingFact
                {
                    Title = "Environmental Impact",
                    Content = "Every ton of recycled plastic saves 5,774 kWh of energy and 16.3 barrels of oil.",
                    ImageUrl = "/images/environment-impact.jpg"
                },
                new RecyclingFact
                {
                    Title = "Community Benefits",
                    Content = "Recycling creates 6x more jobs than sending waste to landfills.",
                    ImageUrl = "/images/community-benefits.jpg"
                }
            };

            return View(facts);
        }

        public IActionResult Guide()
        {
            return View();
        }

        public IActionResult Impact()
        {
            return View();
        }
    }

    public class RecyclingFact
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}