using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Core.Settings.Models;
using Orchard.Settings;

namespace Orchard.Core.Settings.ViewModels {
    public class SiteSettingsPartViewModel  {
        public SiteSettingsPart Site { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IEnumerable<TimeZoneInfo> TimeZones { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Site.ContentItem.Id; }
        }

        public string PageTitleSeparator {
            get { return Site.PageTitleSeparator; }
            set { Site.PageTitleSeparator = value; }
        }

        public string SiteName {
            get { return Site.SiteName; }
            set { Site.SiteName = value; }
        }

        public string SiteCulture {
            get { return Site.SiteCulture; }
            set { Site.SiteCulture = value; }
        }

        public string SuperUser {
            get { return Site.SuperUser; }
            set { Site.SuperUser = value; }
        }

        public ResourceDebugMode ResourceDebugMode {
            get { return Site.ResourceDebugMode; }
            set { Site.ResourceDebugMode = value; }
        }

        public int PageSize {
            get { return Site.PageSize; }
            set { Site.PageSize = value; }
        }

        public string BaseUrl {
            get { return Site.BaseUrl; }
            set { Site.BaseUrl = value; }
        }

        public string TimeZone {
            get { return Site.SiteTimeZone; }
            set { Site.SiteTimeZone = value; }
        }
    }
}
