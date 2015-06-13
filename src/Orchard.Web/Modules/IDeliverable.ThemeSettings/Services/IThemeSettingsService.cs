using System.Collections.Generic;
using Orchard;
using Orchard.Environment.Extensions.Models;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.Services
{
    public interface IThemeSettingsService : IDependency
    {
        ExtensionDescriptor GetTheme(string themeId);
        ThemeProfile GetProfile(int id);
        ThemeProfile GetProfile(string profileName);
        ThemeProfile GetCurrentProfile();
        IEnumerable<ThemeProfile> GetProfiles(string themeId);
        IEnumerable<ThemeProfile> GetAllProfiles();
        ThemeSettingsManifest GetSettingsManifest(string themeId);
        bool HasSettingsManifest(string themeId);
        void SaveProfile(ThemeProfile profile);
        ThemeProfile CloneProfile(ThemeProfile profile);
        void DeleteProfile(int id);
        string SerializeSettings(IEnumerable<KeyValuePair<string, ThemeSetting>> settings);
        IDictionary<string, ThemeSetting> DeserializeSettings(string settings);
    }
}