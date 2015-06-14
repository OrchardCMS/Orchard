using System.Collections.Generic;

namespace IDeliverable.ThemeSettings.Models
{
    public class ThemeSettingsGroupDefinition
    {
        public ThemeSettingsGroupDefinition()
        {
            Settings = new List<ThemeSettingDefinition>();
        }

        public string Name { get; set; }
        public IList<ThemeSettingDefinition> Settings { get; set; }
    }
}