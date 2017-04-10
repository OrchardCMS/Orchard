using System.Web.Routing;
using Orchard.Localization;
namespace Orchard.Themes {
    public class ThemeSelectorResult {
        public int Priority { get; set; }
        public string ThemeName { get; set; }
    }

    public interface IThemeSelector : IDependency {
        ThemeSelectorResult GetTheme(RequestContext context);
        string GetTheme();
        /// <summary>
        /// if CanSet is true,this will be used to set the theme
        /// </summary>
        /// <param name="themeName"></param>
        void SetTheme(string themeName);
        /// <summary>
        /// it is a flag to tell the stysem,the selector either can be seted or not.
        /// </summary>
        bool CanSet { get; }
        /// <summary>
        /// this will tell the system,the theme.txt need contains this Tag
        /// </summary>
        string Tag { get; }
        /// <summary>
        /// a Display name,that will be used in Orchard.Themes.AdminMenu to build a menu
        /// </summary>
        LocalizedString DisplayName { get; }
        /// <summary>
        /// this like a parameter to build a menu
        /// </summary>
        string Name { get; }
    }

}