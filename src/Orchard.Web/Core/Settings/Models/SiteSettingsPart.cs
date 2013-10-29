using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public sealed class SiteSettingsPart : ContentPart, ISite {

        public const int DefaultPageSize = 10;
        public string PageTitleSeparator {
            get { return Get("PageTitleSeparator"); }
            set { Set("PageTitleSeparator", value); }
        }
        public string SiteName {
            get { return Get("SiteName"); }
            set { Set("SiteName", value); }
        }

        public string SiteSalt {
            get { return Get("SiteSalt"); }
            set { Set("SiteSalt", value); }
        }

        public string SuperUser {
            get { return Get("SuperUser"); }
            set { Set("SuperUser", value); }
        }

        public string HomePage {
            get { return Get("HomePage"); }
            set { Set("HomePage", value); }
        }

        public string SiteCulture {
            get { return Get("SiteCulture"); }
            set { Set("SiteCulture", value); }
        }

        public ResourceDebugMode ResourceDebugMode {
            get {
                var value = Get("ResourceDebugMode");
                return String.IsNullOrEmpty(value) ? ResourceDebugMode.Disabled : (ResourceDebugMode)Enum.Parse(typeof(ResourceDebugMode), value);
            }
            set { Set("ResourceDebugMode", value.ToString()); }
        }

        public int PageSize {
            get { return int.Parse(Get("PageSize") ?? "0", CultureInfo.InvariantCulture); }
            set { Set("PageSize", value.ToString(CultureInfo.InvariantCulture)); }
        }

        public string SiteTimeZone {
            get { return Get("SiteTimeZone"); }
            set { Set("SiteTimeZone", value); }
        }

        public string BaseUrl {
            get { return Get("BaseUrl"); }
            set { Set("BaseUrl", value); }
        }
    }
}
