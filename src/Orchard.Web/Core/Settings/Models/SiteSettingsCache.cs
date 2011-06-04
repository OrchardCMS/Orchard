using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public class SiteSettingsCache : ISite {
        private readonly int _id;

        private SiteSettingsPart _siteSettingsPart;

        private string _pageTitleSeparator;
        private string _siteName;
        private string _siteSalt;
        private string _superUser;
        private string _homePage;
        private string _siteCulture;
        private ResourceDebugMode _resourceDebugMode;
        private int _pageSize;
        private string _baseUrl;

        public SiteSettingsCache(ISite site) {
            _id = site.Id;
            _pageTitleSeparator = site.PageTitleSeparator;
            _siteName = site.SiteName;
            _siteSalt = site.SiteSalt;
            _superUser = site.SuperUser;
            _homePage = site.HomePage;
            _siteCulture = site.SiteCulture;
            _resourceDebugMode = site.ResourceDebugMode;
            _pageSize = site.PageSize;
            _baseUrl = site.BaseUrl;
        }

        public int Id {
            get { return _id; }
        }

        public string PageTitleSeparator {
            get { return _pageTitleSeparator; }

            set {
                _pageTitleSeparator = value;
                SiteSettingsPart.PageTitleSeparator = value;
            }
        }

        public string SiteName {
            get { return _siteName; }

            set {
                _siteName = value;
                SiteSettingsPart.SiteName = value;
            }
        }

        public string SiteSalt {
            get { return _siteSalt; }

            set {
                _siteSalt = value;
                SiteSettingsPart.Record.SiteSalt = value;
            }
        }

        public string SuperUser {
            get { return _superUser; }

            set {
                _superUser = value;
                SiteSettingsPart.SuperUser = value;
            }
        }

        public string HomePage {
            get { return _homePage; }

            set {
                _homePage = value;
                SiteSettingsPart.HomePage = value;
            }
        }

        public string SiteCulture {
            get { return _siteCulture; }

            set {
                _siteCulture = value;
                SiteSettingsPart.SiteCulture = value;
            }
        }

        public ResourceDebugMode ResourceDebugMode {
            get { return _resourceDebugMode; }

            set {
                _resourceDebugMode = value;
                SiteSettingsPart.ResourceDebugMode = value;
            }
        }

        public int PageSize {
            get { return _pageSize; }

            set {
                _pageSize = value;
                SiteSettingsPart.PageSize = value;
            }
        }

        public string BaseUrl {
            get { return _baseUrl; }

            set {
                _baseUrl = value;
                SiteSettingsPart.BaseUrl = value;
            }
        }

        public ContentItem ContentItem {
            get { return SiteSettingsPart.ContentItem; }
        }

        private ISiteService SiteService { get; set; }

        private SiteSettingsPart SiteSettingsPart {
            get {
                if (_siteSettingsPart == null) {
                    _siteSettingsPart = SiteService.GetSiteSettingsPart() as SiteSettingsPart;
                }

                return _siteSettingsPart;
            }

            set { _siteSettingsPart = value; }
        }

        public void ResetCache(ISiteService siteService) {
            SiteService = siteService;
            SiteSettingsPart = null;
        }
    }
}