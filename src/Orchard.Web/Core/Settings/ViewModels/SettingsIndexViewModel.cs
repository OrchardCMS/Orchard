using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;
using Orchard.Core.Settings.Models;

namespace Orchard.Core.Settings.ViewModels {
    public class SettingsIndexViewModel  {
        public SiteSettingsPart Site { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public ContentItemViewModel ViewModel { get; set; }
        

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
    }
}
