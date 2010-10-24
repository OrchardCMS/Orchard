using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Settings;

namespace Orchard.Core.Settings.ViewModels {
    public class SiteSettingsPartViewModel  {
        public SiteSettingsPart Site { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Site.ContentItem.Id; }
        }

        public string PageTitleSeparator
        {
            get { return Site.As<SiteSettingsPart>().Record.PageTitleSeparator; }
            set { Site.As<SiteSettingsPart>().Record.PageTitleSeparator = value; }
        }

        public string SiteName {
            get { return Site.As<SiteSettingsPart>().Record.SiteName; }
            set { Site.As<SiteSettingsPart>().Record.SiteName = value; }
        }

        public string SiteCulture {
            get { return Site.As<SiteSettingsPart>().Record.SiteCulture; }
            set { Site.As<SiteSettingsPart>().Record.SiteCulture = value; }
        }

        public string SuperUser {
            get { return Site.As<SiteSettingsPart>().Record.SuperUser; }
            set { Site.As<SiteSettingsPart>().Record.SuperUser = value; }
        }

        public ResourceDebugMode ResourceDebugMode {
            get { return Site.As<SiteSettingsPart>().ResourceDebugMode; }
            set { Site.As<SiteSettingsPart>().ResourceDebugMode = value; }
        }
    }
}
