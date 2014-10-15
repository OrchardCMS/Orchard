using Orchard.Layouts.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Form : Container {
        public override string Category {
            get { return "Form"; }
        }

        public string Name {
            get { return State.Get("FormName"); }
            set { State["FormName"] = value; }
        }

        public bool? EnableClientValidation {
            get { return State.Get("EnableClientValidation").ToBoolean(); }
            set { State["EnableClientValidation"] = value.ToString(); }
        }

        public string Action {
            get { return State.Get("FormAction"); }
            set { State["FormAction"] = value; }
        }

        public string Method {
            get { return State.Get("FormMethod"); }
            set { State["FormMethod"] = value; }
        }

        public bool? StoreSubmission {
            get { return State.Get("StoreSubmission").ToBoolean(); }
            set { State["StoreSubmission"] = value != null ? value.Value.ToString() : null; }
        }

        public bool? CreateContent {
            get { return State.Get("CreateContent").ToBoolean(); }
            set { State["CreateContent"] = value != null ? value.Value.ToString() : null; }
        }

        public string ContentType {
            get { return State.Get("CreateContentType"); }
            set { State["CreateContentType"] = value; }
        }

        public string Publication {
            get { return State.Get("Publication"); }
            set { State["Publication"] = value; }
        }

        public string Notification {
            get { return State.Get("Notification"); }
            set { State["Notification"] = value; }
        }

        public string RedirectUrl {
            get { return State.Get("RedirectUrl"); }
            set { State["RedirectUrl"] = value; }
        }
    }
}