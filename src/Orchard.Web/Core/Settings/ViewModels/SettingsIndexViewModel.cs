using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.Core.Settings.Models;

namespace Orchard.Core.Settings.ViewModels {
    public class SettingsIndexViewModel : AdminViewModel {
        public SiteSettings Site { get; set; }
        public ItemEditorModel EditorModel { get; set; }
        

        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return Site.ContentItem.Id; }
        }

        public string SiteName {
            get { return Site.As<SiteSettings>().Record.SiteName; }
            set { Site.As<SiteSettings>().Record.SiteName = value; }
        }

        public string SuperUser {
            get { return Site.As<SiteSettings>().Record.SuperUser; }
            set { Site.As<SiteSettings>().Record.SuperUser = value; }
        }
    }
}
