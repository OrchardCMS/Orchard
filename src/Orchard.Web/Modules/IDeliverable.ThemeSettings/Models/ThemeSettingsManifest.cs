using System.Collections.Generic;

namespace IDeliverable.ThemeSettings.Models
{
    public class ThemeSettingsManifest
    {
        public ThemeSettingsManifest()
        {
            Groups = new List<ThemeSettingsGroupDefinition>();
        }

        public IList<ThemeSettingsGroupDefinition> Groups { get; set; }
    }
}