using Orchard.Layouts.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Form : Container {
        public override string Category {
            get { return "Forms"; }
        }

        public string Name {
            get { return this.Retrieve("FormName", () => "Untitled"); }
            set { this.Store("FormName", value); }
        }

        public bool? EnableClientValidation {
            get { return this.Retrieve(x => x.EnableClientValidation); }
            set { this.Store(x => x.EnableClientValidation, value); }
        }

        public string Action {
            get { return this.Retrieve<string>("FormAction"); }
            set { this.Store("FormAction", value); }
        }

        public string Method {
            get { return this.Retrieve<string>("FormMethod"); }
            set { this.Store("FormMethod", value); }
        }

        public bool? StoreSubmission {
            get { return this.Retrieve(x => x.StoreSubmission); }
            set { this.Store(x => x.StoreSubmission, value); }
        }

        public bool HtmlEncode {
            get { return this.Retrieve(x => x.HtmlEncode, () => true); }
            set { this.Store(x => x.HtmlEncode, value); }
        }

        public bool? CreateContent {
            get { return this.Retrieve(x => x.CreateContent); }
            set { this.Store(x => x.CreateContent, value); }
        }

        public string FormBindingContentType {
            get { return this.Retrieve(x => x.FormBindingContentType); }
            set { this.Store(x => x.FormBindingContentType, value); }
        }

        public string Publication {
            get { return this.Retrieve(x => x.Publication); }
            set { this.Store(x => x.Publication, value); }
        }

        public string Notification {
            get { return this.Retrieve(x => x.Notification); }
            set { this.Store(x => x.Notification, value); }
        }

        public string RedirectUrl {
            get { return this.Retrieve(x => x.RedirectUrl); }
            set { this.Store(x => x.RedirectUrl, value); }
        }
    }
}