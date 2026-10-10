using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace RVMWMApp.Controllers
{
    public class LanguageController : Controller
    {
        // Accepts codes like "en", "zu", "af", "en-ZA"
        private static readonly Regex ValidLang = new(@"^[a-zA-Z]{2,3}(-[a-zA-Z]{2,4})?$", RegexOptions.Compiled);

        [HttpPost]
        public IActionResult SetLang(string lang, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(lang) || !ValidLang.IsMatch(lang))
            {
                lang = "en";
            }

            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };

            // Your own "lang" cookie (kept so any existing code that reads it still works)
            Response.Cookies.Append("lang", lang, options);

            // The cookie ASP.NET Core's built-in localization middleware reads
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                options);

            // LocalRedirect throws on null or external URLs, so fall back to home
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            return LocalRedirect(returnUrl);
        }
    }
}