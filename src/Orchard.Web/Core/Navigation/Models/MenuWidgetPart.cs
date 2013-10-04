using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuWidgetPart : ContentPart<MenuWidgetPartRecord> {

        public int StartLevel {
            get { return int.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("StartLevel") ?? "0", CultureInfo.InvariantCulture); }
            set {
                this.As<InfosetPart>().Set<MenuWidgetPart>("StartLevel", value.ToString(CultureInfo.InvariantCulture));
                Record.StartLevel = value;
            }
        }

        public int Levels {
            get { return int.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("Levels") ?? "0", CultureInfo.InvariantCulture); }
            set {
                this.As<InfosetPart>().Set<MenuWidgetPart>("Levels", value.ToString(CultureInfo.InvariantCulture));
                Record.Levels = value;
            }
        }

        public bool Breadcrumb {
            get { return bool.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("Breadcrumb") ?? "false"); }
            set {
                this.As<InfosetPart>().Set<MenuWidgetPart>("Breadcrumb", value.ToString());
                Record.Breadcrumb = value;
            }
        }

        public bool AddHomePage {
            get { return bool.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("AddHomePage") ?? "false"); }
            set {
                this.As<InfosetPart>().Set<MenuWidgetPart>("AddHomePage", value.ToString());
                Record.AddHomePage = value;
            }
        }

        public bool AddCurrentPage {
            get { return bool.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("AddCurrentPage") ?? "false"); }
            set {
                this.As<InfosetPart>().Set<MenuWidgetPart>("AddCurrentPage", value.ToString());
                Record.AddCurrentPage = value;
            }
        }
        
        public ContentItemRecord Menu {
            get { return Record.Menu; }
            set { Record.Menu = value; }
        }

        public bool ShowFullMenu {
            get { return bool.Parse(this.As<InfosetPart>().Get<MenuWidgetPart>("ShowFullMenu") ?? "false"); }
            set { this.As<InfosetPart>().Set<MenuWidgetPart>("ShowFullMenu", value.ToString()); }
        }
    }
}