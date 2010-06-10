using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.ViewModels;
using Orchard.Core.Settings.Models;

namespace Orchard.Core.Settings.ViewModels {
    public class SettingsIndexViewModel : BaseViewModel {
        public SiteSettings Site { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public ContentItemViewModel ViewModel { get; set; }
        

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Site.ContentItem.Id; }
        }

        public string PageTitleSeparator
        {
            get { return Site.As<SiteSettings>().Record.PageTitleSeparator; }
            set { Site.As<SiteSettings>().Record.PageTitleSeparator = value; }
        }

        public string SiteName {
            get { return Site.As<SiteSettings>().Record.SiteName; }
            set { Site.As<SiteSettings>().Record.SiteName = value; }
        }

        public string SiteCulture {
            get { return Site.As<SiteSettings>().Record.SiteCulture; }
            set { Site.As<SiteSettings>().Record.SiteCulture = value; }
        }

        public string SuperUser {
            get { return Site.As<SiteSettings>().Record.SuperUser; }
            set { Site.As<SiteSettings>().Record.SuperUser = value; }
        }
    }
}
