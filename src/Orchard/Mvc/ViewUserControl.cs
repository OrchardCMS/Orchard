using System.Web;
using System.Web.Mvc;
using Autofac;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc {
    public class ViewUserControl<TModel> : System.Web.Mvc.ViewUserControl<TModel> {
        private object _display;
        private Localizer _localizer = NullLocalizer.Instance;

        public Localizer T { get { return _localizer; } }
        public dynamic Display { get { return _display; } }
        public IDisplayHelperFactory DisplayHelperFactory { get; set; }

        public IAuthorizer Authorizer { get; set; }

        public override void RenderView(ViewContext viewContext) {
            var workContext = viewContext.GetWorkContext();
            workContext.Resolve<IComponentContext>().InjectUnsetProperties(this);

            _localizer = LocalizationUtilities.Resolve(viewContext, AppRelativeVirtualPath);
            _display = DisplayHelperFactory.CreateHelper(viewContext, this);

            base.RenderView(viewContext);
        }

        public MvcHtmlString H(string value) {
            return MvcHtmlString.Create(Html.Encode(value));
        }

        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }
    }

    public class ViewUserControl : ViewUserControl<dynamic> {
    }
}
