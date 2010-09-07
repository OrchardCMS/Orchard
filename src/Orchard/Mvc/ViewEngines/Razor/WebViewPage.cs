using Autofac;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc.ViewEngines.Razor {

    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel> {
        private object _display;
        private object _new;
        private Localizer _localizer = NullLocalizer.Instance;


        public Localizer T { get { return _localizer; } }
        public dynamic Display { get { return _display; } }
        public dynamic New { get { return _new; } }
        public IDisplayHelperFactory DisplayHelperFactory { get; set; }
        public IShapeHelperFactory ShapeHelperFactory { get; set; }

        public IAuthorizer Authorizer { get; set; }

        public override void InitHelpers() {
            base.InitHelpers();

            var workContext = ViewContext.GetWorkContext();
            workContext.Resolve<IComponentContext>().InjectUnsetProperties(this);

            _localizer = LocalizationUtilities.Resolve(ViewContext, VirtualPath);
            _display = DisplayHelperFactory.CreateHelper(ViewContext, this);
            _new = ShapeHelperFactory.CreateHelper();
        }


        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }

    }

    public abstract class WebViewPage : WebViewPage<dynamic> {
    }

}
