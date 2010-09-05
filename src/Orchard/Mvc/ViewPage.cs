using System.Web.Mvc;
using Autofac;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc {
    public class ViewPage<TModel> : System.Web.Mvc.ViewPage<TModel> {
        private object _display;
        private Localizer _localizer = NullLocalizer.Instance;

        public Localizer T { get { return _localizer; } }
        public dynamic Display { get { return _display; } }
        public IDisplayHelperFactory DisplayHelperFactory { get; set; }

        public IAuthorizer Authorizer { get; set; }

        public override void InitHelpers() {
            base.InitHelpers();
            
            var workContext = ViewContext.GetWorkContext();
            workContext.Resolve<IComponentContext>().InjectUnsetProperties(this);

            _localizer = LocalizationUtilities.Resolve(ViewContext, AppRelativeVirtualPath);
            _display = DisplayHelperFactory.CreateHelper(ViewContext, this);
        }

        public MvcHtmlString H(string value) {
            return MvcHtmlString.Create(Html.Encode(value));
        }

        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }
    }
}