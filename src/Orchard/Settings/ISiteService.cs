namespace Orchard.Settings {
    public interface ISiteService : IDependency {
        ISite GetSiteSettings();
        ISite GetSiteSettingsPart();
    }
}
