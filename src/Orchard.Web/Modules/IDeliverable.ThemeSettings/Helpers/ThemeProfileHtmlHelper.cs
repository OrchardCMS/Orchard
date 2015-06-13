using System.Web.Mvc;
using Orchard.Mvc.Html;
using IDeliverable.ThemeSettings.Models;
using IDeliverable.ThemeSettings.Services;

namespace IDeliverable.ThemeSettings.Helpers
{
    public static class ThemeProfileHtmlHelper
    {
        public static ThemeProfile GetCurrentThemeProfile(this HtmlHelper html)
        {
            IThemeSettingsService service;
            var workContext = html.GetWorkContext();
            return workContext.TryResolve(out service) ? service.GetCurrentProfile() : null;
        }

        public static string GetThemeSetting(this HtmlHelper html, string name, string defaultValue = null)
        {
            var profile = GetCurrentThemeProfile(html);
            return GetThemeSetting(html, profile, name, defaultValue);
        }

        public static string GetThemeSetting(this HtmlHelper html, ThemeProfile profile, string name, string defaultValue = null)
        {
            return profile != null && profile.Settings.ContainsKey(name) ? profile.Settings[name].Value : defaultValue;
        }
    }
}