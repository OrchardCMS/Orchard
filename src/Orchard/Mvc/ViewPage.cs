using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Autofac;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Spooling;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Mvc {
    public class ViewPage<TModel> : System.Web.Mvc.ViewPage<TModel>, IOrchardViewPage {
        private object _display;
        private Localizer _localizer = NullLocalizer.Instance;
        private WorkContext _workContext;

        public Localizer T { get { return _localizer; } }
        public dynamic Display { get { return _display; } }
        public WorkContext WorkContext { get { return _workContext; } }

        public IDisplayHelperFactory DisplayHelperFactory { get; set; }

        public IAuthorizer Authorizer { get; set; }

        public override void InitHelpers() {
            base.InitHelpers();

            _workContext = ViewContext.GetWorkContext();
            _workContext.Resolve<IComponentContext>().InjectUnsetProperties(this);

            _localizer = LocalizationUtilities.Resolve(ViewContext, AppRelativeVirtualPath);
            _display = DisplayHelperFactory.CreateHelper(ViewContext, this);
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
            return new CaptureScope(Writer, callback);
        }

        public class CaptureScope : IDisposable {
            private readonly HtmlTextWriter _context;
            private readonly Action<IHtmlString> _callback;
            private readonly TextWriter _oldWriter;
            private readonly HtmlStringWriter _writer;

            public CaptureScope(HtmlTextWriter context, Action<IHtmlString> callback) {
                _context = context;
                _oldWriter = _context.InnerWriter;
                _callback = callback;
                _context.InnerWriter = _writer = new HtmlStringWriter();
            }

            public void Dispose() {
                _callback(_writer);
                _context.InnerWriter = _oldWriter;
            }
        }

    }

    public class ViewPage : ViewPage<dynamic> {
    }
}