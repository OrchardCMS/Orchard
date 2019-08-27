using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
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

            var baseUrl = new Func<string>(() => {
                var s = siteService.GetSiteSettings().BaseUrl;

                // When Setup is running from the command line, no BaseUrl exists yet.
                return string.IsNullOrEmpty(s) ? "http://localhost" : s;
            });

            var httpContextBase = new HttpContextPlaceholder(baseUrl);

            return httpContextBase;
        }

        static RequestContext RequestContextFactory(IComponentContext context) {

            var httpContext = HttpContextBaseFactory(context);

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
                return ((HttpContextPlaceholder)httpContext).RequestContext;
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
            private HttpRequestPlaceholder _request;
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

            public override HttpRequestBase Request {

                // Note: To fully resolve the baseUrl, some factories are needed (HttpContextBase, RequestContext...),
                // so, doing this in such a factory creates a circular dependency (see HttpContextBase factory comments).

                // When rendering a view in a background task, an Html Helper can access HttpContext.Current directly,
                // so, here we create a fake HttpContext based on the baseUrl, and use it to update HttpContext.Current.
                // We cannot do this before in a factory (see note above), anyway, by doing this on each Request access,
                // we have a better chance to maintain the HttpContext.Current state even with some asynchronous code.

                get {
                    if (_httpContext == null) {
                        var httpContext = new HttpContext(new HttpRequest("", _baseUrl.Value, ""), new HttpResponse(new StringWriter()));
                        httpContext.Items[IsBackgroundHttpContextKey] = true;
                        _httpContext = httpContext;
                    }

                    if (HttpContext.Current != _httpContext)
                        HttpContext.Current = _httpContext;

                    if (_request == null) {
                        _request = new HttpRequestPlaceholder(this, _baseUrl);
                    }
                    return _request;
                }
            }

            internal RequestContext RequestContext {

                // Uses the Request object but without creating an HttpContext which would need to resolve the baseUrl,
                // so, can be used by the RequestContext factory without creating a circular dependency (see note above).

                get {
                    if (_request == null) {
                        _request = new HttpRequestPlaceholder(this, _baseUrl);
                    }
                    return _request.RequestContext;
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
            private RequestContext _requestContext;
            private readonly Lazy<string> _baseUrl;
            private readonly NameValueCollection _queryString = new NameValueCollection();
            private Uri _uri;

            public HttpRequestPlaceholder(HttpContextBase httpContext, Lazy<string> baseUrl) {
                _httpContext = httpContext;
                _baseUrl = baseUrl;
            }

            public override RequestContext RequestContext {
                get {
                    if (_requestContext == null) {
                        _requestContext = new RequestContext(_httpContext, new RouteData());
                    }
                    return _requestContext;
                }
            }

            public override NameValueCollection QueryString {
                get {
                    return _queryString;
                }
            }

            public override string RawUrl {
                get {
                    return Url.OriginalString;
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
                    if (_uri == null) {
                        _uri = new Uri(_baseUrl.Value);
                    }
                    return _uri;
                }
            }

            public override NameValueCollection Headers {
                get {
                    return new NameValueCollection { { "Host", Url.Authority } };
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
                    return Url.LocalPath;
                }
            }

            public override NameValueCollection ServerVariables {
                get {
                    return new NameValueCollection {
                        { "SERVER_PORT", Url.Port.ToString(CultureInfo.InvariantCulture) },
                        { "HTTP_HOST", Url.Authority.ToString(CultureInfo.InvariantCulture) },

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

            public override string MapPath(string path) {
                return HostingEnvironment.MapPath(path);
            }
        }
    }
}
