using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Core.Settings.Models {
    public sealed class SiteSettingsPart : ContentPart, ISite {

        public const int DefaultPageSize = 10;

        public string PageTitleSeparator {
            get { return this.Retrieve(x => x.PageTitleSeparator); }
            set { this.Store(x => x.PageTitleSeparator, value); }
        }
        public string SiteName {
            get { return this.Retrieve(x => x.SiteName); }
            set { this.Store(x => x.SiteName, value); }
        }

        public string SiteSalt {
            get { return this.Retrieve(x => x.SiteSalt); }
            set { this.Store(x => x.SiteSalt, value); }
        }

        public string SuperUser {
            get { return this.Retrieve(x => x.SuperUser); }
            set { this.Store(x => x.SuperUser, value); }
        }

        public string HomePage {
            get { return this.Retrieve(x => x.HomePage); }
            set { this.Store(x => x.HomePage, value); }
        }

        public string SiteCulture {
            get { return this.Retrieve(x => x.SiteCulture); }
            set { this.Store(x => x.SiteCulture, value); }
        }

		public string SiteCalendar {
            get { return this.Retrieve(x => x.SiteCalendar); }
            set { this.Store(x => x.SiteCalendar, value); }
        }

        public ResourceDebugMode ResourceDebugMode {
            get { return this.Retrieve(x => x.ResourceDebugMode); }
            set { this.Store(x => x.ResourceDebugMode, value); }
        }
        
        public bool UseCdn {
            get { return this.Retrieve(x => x.UseCdn); }
            set { this.Store(x=> x.UseCdn, value); }
        }

        public int PageSize {
            get { return this.Retrieve(x => x.PageSize, DefaultPageSize); }
            set { this.Store(x => x.PageSize, value); }
        }

        public int MaxPageSize {
            get { return this.Retrieve(x => x.MaxPageSize); }
            set { this.Store(x => x.MaxPageSize, value); }
        }

        public int MaxPagedCount {
            get { return this.Retrieve(x => x.MaxPagedCount); }
            set { this.Store(x => x.MaxPagedCount, value); }
        }

        public string SiteTimeZone {
            get { return this.Retrieve(x => x.SiteTimeZone); }
            set { this.Store(x => x.SiteTimeZone, value); }
        }

        public string BaseUrl {
            get { return this.Retrieve(x => x.BaseUrl); }
            set { this.Store(x => x.BaseUrl, value); }
        }
    }
}
