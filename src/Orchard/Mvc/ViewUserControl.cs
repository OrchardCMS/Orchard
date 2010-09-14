using System;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Spooling;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc {
    public class ViewUserControl<TModel> : System.Web.Mvc.ViewUserControl<TModel>,IOrchardViewPage {
        private object _display;
        private object _new;
        private Localizer _localizer = NullLocalizer.Instance;
        private WorkContext _workContext;

        public Localizer T { get { return _localizer; } }
        public dynamic Display { get { return _display; } }
        public dynamic New { get { return _new; } }
        public WorkContext WorkContext { get { return _workContext; } }
        
        public IDisplayHelperFactory DisplayHelperFactory { get; set; }
        public IShapeHelperFactory ShapeHelperFactory { get; set; }

        public IAuthorizer Authorizer { get; set; }

        public override void RenderView(ViewContext viewContext) {
            _workContext = viewContext.GetWorkContext();
            _workContext.Resolve<IComponentContext>().InjectUnsetProperties(this);

            _localizer = LocalizationUtilities.Resolve(viewContext, AppRelativeVirtualPath);
            _display = DisplayHelperFactory.CreateHelper(viewContext, this);
            _new = ShapeHelperFactory.CreateHelper();

            base.RenderView(viewContext);
        }

        public MvcHtmlString H(string value) {
            return MvcHtmlString.Create(Html.Encode(value));
        }

        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }

        public IHtmlString DisplayChildren(dynamic shape) {
            var writer = new HtmlStringWriter();
            foreach (var item in shape) {
                writer.Write(Display(item));
            }
            return writer;
        }

        public IDisposable Capture(Action<IHtmlString> callback) {
            throw new NotImplementedException();
        }

    }

    public class ViewUserControl : ViewUserControl<dynamic> {
    }
}
