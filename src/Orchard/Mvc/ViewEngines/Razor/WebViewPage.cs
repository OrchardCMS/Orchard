using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Autofac;
using ClaySharp;
using Orchard.ContentManagement;
using Orchard.Data.Migration;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc.ViewEngines.Razor {

    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel> {
        private object _display;
        private Localizer _localizer = NullLocalizer.Instance;
        private IEnumerable<Action> _contexturalizers = Enumerable.Empty<Action>();


        public Localizer T { get { return _localizer; } }
        public dynamic Display { get { return _display; } }
        public IDisplayHelperFactory DisplayHelperFactory { get; set; }

        public IAuthorizer Authorizer { get; set; }

        public override void InitHelpers() {
            base.InitHelpers();

            var workContext = ViewContext.GetWorkContext();
            workContext.Service<IContainer>().InjectUnsetProperties(this);

            _localizer = LocalizationUtilities.Resolve(ViewContext, VirtualPath);
            _display = DisplayHelperFactory.CreateHelper(ViewContext, this);
        }


        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }

    }

    public abstract class WebViewPage : WebViewPage<object> {
    }

}
