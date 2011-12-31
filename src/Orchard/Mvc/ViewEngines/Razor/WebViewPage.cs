using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Mvc.Spooling;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Resources;

namespace Orchard.Mvc.ViewEngines.Razor {

    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>, IOrchardViewPage {
        private ScriptRegister _scriptRegister;
        private ResourceRegister _stylesheetRegister;
        private Localizer _localizer = NullLocalizer.Instance;
        private object _display;
        private object _layout;
        private WorkContext _workContext;

        public Localizer T { 
            get {
                // first time used, create it
                if(_localizer == NullLocalizer.Instance) {
                
                    // if the Model is a shape, get localization scopes from binding sources
                    // e.g., Logon.cshtml in a theme, overriging Users/Logon.cshtml, needs T to 
                    // fallback to the one in Users
                    var shape = Model as IShape;
                    if(shape != null && shape.Metadata.BindingSources.Count > 1) {
                        var localizers = shape.Metadata.BindingSources.Reverse().Select(scope => LocalizationUtilities.Resolve(ViewContext, scope)).ToList();
                        _localizer = (text, args) => { 
                            foreach(var localizer in localizers) {
                                var hint = localizer(text, args);
                                // if not localized using this scope, use next scope
                                if(hint.Text != text) {
                                    return hint;
                                }
                            }

                            // no localization found, return default value
                            return new LocalizedString(text, VirtualPath, text, args);
                        };
                    }
                    else {
                        // not a shape, use the VirtualPath as scope
                        _localizer = LocalizationUtilities.Resolve(ViewContext, VirtualPath);
                    }
                }

                return _localizer;
            } 
        }

        public dynamic Display { get { return _display; } }
        // review: (heskew) is it going to be a problem?
        public new dynamic Layout { get { return _layout; } }
        public WorkContext WorkContext { get { return _workContext; } }

        public dynamic New { get { return ShapeFactory; } }

        private IDisplayHelperFactory _displayHelperFactory;
        public IDisplayHelperFactory DisplayHelperFactory {
            get {
                return _displayHelperFactory ?? (_displayHelperFactory = _workContext.Resolve<IDisplayHelperFactory>());
            }
        }

        private IShapeFactory _shapeFactory;
        public IShapeFactory ShapeFactory {
            get {
                return _shapeFactory ?? (_shapeFactory = _workContext.Resolve<IShapeFactory>());
            }
        }

        private IAuthorizer _authorizer;
        public IAuthorizer Authorizer { 
            get {
                return _authorizer ?? (_authorizer = _workContext.Resolve<IAuthorizer>());
            }
        }

        public ScriptRegister Script {
            get {
                return _scriptRegister ??
                    (_scriptRegister = new WebViewScriptRegister(this, Html.ViewDataContainer, ResourceManager));
            }
        }

        private IResourceManager _resourceManager;
        public IResourceManager ResourceManager {
            get { return _resourceManager ?? (_resourceManager = _workContext.Resolve<IResourceManager>()); }
        }

        public ResourceRegister Style {
            get {
                return _stylesheetRegister ??
                    (_stylesheetRegister = new ResourceRegister(Html.ViewDataContainer, ResourceManager, "stylesheet"));
            }
        }

        public void RegisterImageSet(string imageSet, string style = "", int size = 16) {
            // hack to fake the style "alternate" for now so we don't have to change stylesheet names when this is hooked up
            // todo: (heskew) deal in shapes so we have real alternates 
            var imageSetStylesheet = !string.IsNullOrWhiteSpace(style)
                ? string.Format("{0}-{1}.css", imageSet, style)
                : string.Format("{0}.css", imageSet);
            Style.Include(imageSetStylesheet);
        }

        public virtual void RegisterLink(LinkEntry link) {
            ResourceManager.RegisterLink(link);
        }

        public void SetMeta(string name, string content) {
            SetMeta(new MetaEntry { Name = name, Content = content });
        }

        public virtual void SetMeta(MetaEntry meta) {
            ResourceManager.SetMeta(meta);
        }

        public void AppendMeta(string name, string content, string contentSeparator) {
            AppendMeta(new MetaEntry { Name = name, Content = content }, contentSeparator);
        }

        public virtual void AppendMeta(MetaEntry meta, string contentSeparator) {
            ResourceManager.AppendMeta(meta, contentSeparator);
        }

        public override void InitHelpers() {
            base.InitHelpers();

            _workContext = ViewContext.GetWorkContext();
            
            _display = DisplayHelperFactory.CreateHelper(ViewContext, this);
            _layout = _workContext.Layout;
        }

        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }

        public bool HasText(object thing) {
            return !string.IsNullOrWhiteSpace(Convert.ToString(thing));
        }

        public OrchardTagBuilder Tag(dynamic shape, string tagName) {
            return Html.GetWorkContext().Resolve<ITagBuilderFactory>().Create(shape, tagName);
        }

        public IHtmlString DisplayChildren(dynamic shape) {
            var writer = new HtmlStringWriter();
            foreach (var item in shape) {
                writer.Write(Display(item));
            }
            return writer;
        }

        public IDisposable Capture(Action<IHtmlString> callback) {
            return new CaptureScope(this, callback);
        }

        class CaptureScope : IDisposable {
            readonly WebPageBase _viewPage;
            readonly Action<IHtmlString> _callback;

            public CaptureScope(WebPageBase viewPage, Action<IHtmlString> callback) {
                _viewPage = viewPage;
                _callback = callback;
                _viewPage.OutputStack.Push(new HtmlStringWriter());
            }

            void IDisposable.Dispose() {
                var writer = (HtmlStringWriter)_viewPage.OutputStack.Pop();
                _callback(writer);
            }
        }

        class WebViewScriptRegister : ScriptRegister {
            private readonly WebPageBase _viewPage;

            public WebViewScriptRegister(WebPageBase viewPage, IViewDataContainer container, IResourceManager resourceManager)
                : base(container, resourceManager) {
                _viewPage = viewPage;
            }

            public override IDisposable Head() {
                return new CaptureScope(_viewPage, s => ResourceManager.RegisterHeadScript(s.ToString()));
            }

            public override IDisposable Foot() {
                return new CaptureScope(_viewPage, s => ResourceManager.RegisterFootScript(s.ToString()));
            }
        }
    }

    public abstract class WebViewPage : WebViewPage<dynamic> {
    }
}
