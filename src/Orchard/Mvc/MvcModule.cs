using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Instrumentation;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Routes;
using Orchard.Settings;

namespace Orchard.Mvc {
    public class MvcModule : Module {
        public const string IsBackgroundHttpContextKey = "IsBackgroundHttpContext";

        protected override void Load(ContainerBuilder moduleBuilder) {
            moduleBuilder.RegisterType<ShellRoute>().InstancePerDependency();

            moduleBuilder.Register(HttpContextBaseFactory).As<HttpContextBase>().InstancePerDependency();
            moduleBuilder.Register(RequestContextFactory).As<RequestContext>().InstancePerDependency();
            moduleBuilder.Register(UrlHelperFactory).As<UrlHelper>().InstancePerDependency();
        }

        static HttpContextBase HttpContextBaseFactory(IComponentContext context) {

            var httpContext = context.Resolve<IHttpContextAccessor>().Current();
            if (httpContext != null)
                return httpContext;

            var siteService = context.Resolve<ISiteService>();

            // Wrapping the code accessing the SiteSettings in a function that will be executed later (in HttpContextPlaceholder),
            // so that the RequestContext will have been established when the time comes to actually load the site settings,
            // which requires activating the Site content item, which in turn requires a UrlHelper, which in turn requires a RequestContext,
            // thus preventing a StackOverflowException.
            var baseUrl = new Func<string>(() => siteService.GetSiteSettings().BaseUrl);
            var httpContextBase = new HttpContextPlaceholder(baseUrl);

            return httpContextBase;
        }

        static RequestContext RequestContextFactory(IComponentContext context) {
            var httpContextAccessor = context.Resolve<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.Current();

            if (!httpContext.IsBackgroundContext()) {

                var mvcHandler = httpContext.Handler as MvcHandler;
                if (mvcHandler != null) {
                    return mvcHandler.RequestContext;
                }

                var hasRequestContext = httpContext.Handler as IHasRequestContext;
                if (hasRequestContext != null) {
                    if (hasRequestContext.RequestContext != null)
                        return hasRequestContext.RequestContext;
                }
            }
            else if (httpContext is HttpContextPlaceholder) {
                if (((HttpContextPlaceholder)httpContext).IsResolved)
                    return httpContext.Request.RequestContext;
            }
            else if (httpContext == null) {
                httpContext = HttpContextBaseFactory(context);
            }

            return new RequestContext(httpContext, new RouteData());
        }

        static UrlHelper UrlHelperFactory(IComponentContext context) {
            return new UrlHelper(context.Resolve<RequestContext>(), context.Resolve<RouteCollection>());
        }

        /// <summary>
        /// Standin context for background tasks.
        /// </summary>
        public class HttpContextPlaceholder : HttpContextBase, IDisposable {
            private HttpContext _httpContext;
            private readonly Lazy<string> _baseUrl;
            private readonly IDictionary _items = new Dictionary<object, object>();

            public HttpContextPlaceholder(Func<string> baseUrl) {
                _baseUrl = new Lazy<string>(baseUrl);
            }

            public void Dispose() {
                _httpContext = null;
                if (HttpContext.Current != null)
                    HttpContext.Current = null;
            }
 
            public bool IsResolved {
                get {
                    return _baseUrl.IsValueCreated;
                }
            }

            public override HttpRequestBase Request {
                get {

                    if (_httpContext == null) {
                        var httpContext = new HttpContext(new HttpRequest("", _baseUrl.Value, ""), new HttpResponse(new StringWriter()));
                        httpContext.Items[IsBackgroundHttpContextKey] = true;
                        _httpContext = httpContext;
                    }

                    if (HttpContext.Current != _httpContext)
                        HttpContext.Current = _httpContext;
                    return new HttpRequestPlaceholder(this, new Uri(_baseUrl.Value));
                }
            }

            public override HttpSessionStateBase Session {
                get { return new HttpSessionStatePlaceholder(); }
            }

            public override IHttpHandler Handler { get; set; }

            public override HttpResponseBase Response {
                get { return new HttpResponsePlaceholder(); }
            }

            public override IDictionary Items {
                get { return _items; }
            }

            public override PageInstrumentationService PageInstrumentation {
                get { return new PageInstrumentationService(); }
            }

            public override Cache Cache {
                get { return HttpRuntime.Cache; }
            }

            public override HttpServerUtilityBase Server {
                get { return new HttpServerUtilityPlaceholder(); }
            }

            public override object GetService(Type serviceType) {
                return null;
            }
        }

        public class HttpSessionStatePlaceholder : HttpSessionStateBase {
            public override object this[string name] {
                get {
                    return null;
                }
            }
        }

        public class HttpResponsePlaceholder : HttpResponseBase {
            public override string ApplyAppPathModifier(string virtualPath) {
                return virtualPath;
            }

            public override HttpCookieCollection Cookies {
                get {
                    return new HttpCookieCollection();
                }
            }
        }

        /// <summary>
        /// standin context for background tasks. 
        /// </summary>
        public class HttpRequestPlaceholder : HttpRequestBase {
            private HttpContextBase _httpContext;
            private readonly Uri _uri;

            public HttpRequestPlaceholder(HttpContextBase httpContext, Uri uri) {
                _httpContext = httpContext;
                _uri = uri;
            }

            public override RequestContext RequestContext {
                get {
                    return new RequestContext(_httpContext, new RouteData());
                }
            }

            public override NameValueCollection QueryString {
                get {
                    return new NameValueCollection();
                }
            }

            public override string RawUrl {
                get {
                    return _uri.OriginalString;
                }
            }

            /// <summary>
            /// anonymous identity provided for background task.
            /// </summary>
            public override bool IsAuthenticated {
                get { return false; }
            }

            /// <summary>
            /// Create an anonymous ID the same way as ASP.NET would.
            /// Some users of an HttpRequestPlaceHolder object could expect this,
            /// say CookieCultureSelector from module Orchard.CulturePicker.
            /// </summary>
            public override string AnonymousID {
                get {
                    return Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
                }
            }

            // empty collection provided for background operation
            public override NameValueCollection Form {
                get {
                    return new NameValueCollection();
                }
            }

            public override Uri Url {
                get {
                    return _uri;
                }
            }

            public override NameValueCollection Headers {
                get {
                    return new NameValueCollection { { "Host", _uri.Authority } };
                }
            }

            public override string HttpMethod {
                get {
                    return "";
                }
            }

            public override NameValueCollection Params {
                get {
                    return new NameValueCollection();
                }
            }

            public override string AppRelativeCurrentExecutionFilePath {
                get {
                    return "~/";
                }
            }

            public override string ApplicationPath {
                get {
                    return _uri.LocalPath;
                }
            }

            public override NameValueCollection ServerVariables {
                get {
                    return new NameValueCollection {
                        { "SERVER_PORT", _uri.Port.ToString(CultureInfo.InvariantCulture) },
                        { "HTTP_HOST", _uri.Authority.ToString(CultureInfo.InvariantCulture) },
                        
                    };
                }
            }

            public override HttpCookieCollection Cookies {
                get {
                    return new HttpCookieCollection();
                }
            }

            public override bool IsLocal {
                get { return true; }
            }

            public override string Path {
                get { return "/"; }
            }

            public override string UserAgent {
                get {
                    return "Placeholder";
                }
            }

            public override string UserHostAddress {
                get {
                    return "127.0.0.1";
                }
            }

            public override string[] UserLanguages {
                get {
                    return new string[0];
                }
            }

            public override HttpBrowserCapabilitiesBase Browser {
                get {
                    return new HttpBrowserCapabilitiesPlaceholder();
                }
            }
        }

        public class HttpBrowserCapabilitiesPlaceholder : HttpBrowserCapabilitiesBase {
            public override string this[string key] {
                get {
                    return "";
                }
            }

            public override bool IsMobileDevice { get { return false; } }
            public override string Browser { get { return "Placeholder"; } }
            public override bool Cookies { get { return true; } }
            public override ArrayList Browsers { get { return new ArrayList(); } }
        }

        public class HttpServerUtilityPlaceholder : HttpServerUtilityBase {
            public override int ScriptTimeout { get; set; }
        }
    }
}
