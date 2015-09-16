﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.Caching;
using System.Web.Instrumentation;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Orchard.Mvc.Routes;
using Orchard.Settings;
using Orchard.Exceptions;

namespace Orchard.Mvc {
    public class MvcModule : Module {

        protected override void Load(ContainerBuilder moduleBuilder) {
            moduleBuilder.RegisterType<ShellRoute>().InstancePerDependency();

            moduleBuilder.Register(HttpContextBaseFactory).As<HttpContextBase>().InstancePerDependency();
            moduleBuilder.Register(RequestContextFactory).As<RequestContext>().InstancePerDependency();
            moduleBuilder.Register(UrlHelperFactory).As<UrlHelper>().InstancePerDependency();
        }

        private static bool IsRequestValid() {
            if (HttpContext.Current == null)
                return false;

            try {
                // The "Request" property throws at application startup on IIS integrated pipeline mode.
                var req = HttpContext.Current.Request;
            }
            catch (Exception ex) {
                if (ex.IsFatal()) {
                    throw;
                }

                return false;
            }

            return true;
        }

        static HttpContextBase HttpContextBaseFactory(IComponentContext context) {
            if (IsRequestValid()) {
                return new HttpContextWrapper(HttpContext.Current);
            }

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
            if (httpContext != null) {

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
            else {
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
        public class HttpContextPlaceholder : HttpContextBase {
            private readonly Lazy<string> _baseUrl;
            private readonly IDictionary _items = new Dictionary<object, object>();

            public HttpContextPlaceholder(Func<string> baseUrl) {
                _baseUrl = new Lazy<string>(baseUrl);
            }

            public override HttpRequestBase Request {
                get { return new HttpRequestPlaceholder(new Uri(_baseUrl.Value)); }
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

            public override object GetService(Type serviceType) {
                return null;
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
            private readonly Uri _uri;

            public HttpRequestPlaceholder(Uri uri) {
                _uri = uri;
            }

            /// <summary>
            /// anonymous identity provided for background task.
            /// </summary>
            public override bool IsAuthenticated {
                get { return false; }
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
    }
}