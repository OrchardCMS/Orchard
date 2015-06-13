using System.Collections.Generic;

namespace IDeliverable.ThemeSettings.Models
{
    public class ThemeProfile
    {
        public ThemeProfile()
        {
            Settings = new Dictionary<string, ThemeSetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Theme { get; set; }
        public IDictionary<string, ThemeSetting> Settings { get; set; }
        public bool IsCurrent { get; set; }
    }
}