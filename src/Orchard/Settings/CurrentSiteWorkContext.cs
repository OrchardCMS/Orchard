namespace Orchard.Settings {
    public class CurrentSiteWorkContext : IWorkContextStateProvider {
        private readonly ISiteService _siteService;

        public CurrentSiteWorkContext(ISiteService siteService) {
            _siteService = siteService;
        }

        public T Get<T>(string name) {
            if (name == "CurrentSite")
                return (T)_siteService.GetSiteSettings();
            return default(T);
        }
    }
}
