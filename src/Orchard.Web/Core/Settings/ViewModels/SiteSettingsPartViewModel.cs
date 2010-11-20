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
            get { return Site.Record.PageTitleSeparator; }
            set { Site.Record.PageTitleSeparator = value; }
        }

        public string SiteName {
            get { return Site.Record.SiteName; }
            set { Site.Record.SiteName = value; }
        }

        public string SiteCulture {
            get { return Site.Record.SiteCulture; }
            set { Site.Record.SiteCulture = value; }
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
