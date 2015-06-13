using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.Services
{
    public class ThemeSettingsValueProvider : DictionaryValueProvider<string>
    {
        public ThemeSettingsValueProvider(IEnumerable<KeyValuePair<string, ThemeSetting>> settings)
            : base(settings.ToDictionary(x => x.Key, x => x.Value.Value), CultureInfo.CurrentCulture)
        {
        }
    }
}