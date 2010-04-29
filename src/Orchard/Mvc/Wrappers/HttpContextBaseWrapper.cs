using System;
using System.Collections;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Profile;

namespace Orchard.Mvc.Wrappers {
    public abstract class HttpContextBaseWrapper : HttpContextBase {
        protected readonly HttpContextBase _httpContextBase;

        protected HttpContextBaseWrapper(HttpContextBase httpContextBase) {
            _httpContextBase = httpContextBase;
        }

        public override void AddError(Exception errorInfo) {
            _httpContextBase.AddError(errorInfo);
        }

        public override void ClearError() {
            _httpContextBase.ClearError();
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey) {
            return _httpContextBase.GetGlobalResourceObject(classKey, resourceKey);
        }

        public override object GetGlobalResourceObject(string classKey, string resourceKey, CultureInfo culture) {
            return _httpContextBase.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey) {
            return _httpContextBase.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public override object GetLocalResourceObject(string virtualPath, string resourceKey, CultureInfo culture) {
            return _httpContextBase.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public override object GetSection(string sectionName) {
            return _httpContextBase.GetSection(sectionName);
        }

        public override object GetService(Type serviceType) {
            return ((IServiceProvider)_httpContextBase).GetService(serviceType);
        }

        public override void RewritePath(string path) {
            _httpContextBase.RewritePath(path);
        }

        public override void RewritePath(string path, bool rebaseClientPath) {
            _httpContextBase.RewritePath(path, rebaseClientPath);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString) {
            _httpContextBase.RewritePath(filePath, pathInfo, queryString);
        }

        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) {
            _httpContextBase.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }

        public override Exception[] AllErrors {
            get {
                return _httpContextBase.AllErrors;
            }
        }

        public override HttpApplicationStateBase Application {
            get {
                return _httpContextBase.Application;
            }
        }

        public override HttpApplication ApplicationInstance {
            get {
                return _httpContextBase.ApplicationInstance;
            }
            set {
                _httpContextBase.ApplicationInstance = value;
            }
        }

        public override Cache Cache {
            get {
                return _httpContextBase.Cache;
            }
        }

        public override IHttpHandler CurrentHandler {
            get {
                return _httpContextBase.CurrentHandler;
            }
        }

        public override RequestNotification CurrentNotification {
            get {
                return _httpContextBase.CurrentNotification;
            }
        }

        public override Exception Error {
            get {
                return _httpContextBase.Error;
            }
        }

        public override IHttpHandler Handler {
            get {
                return _httpContextBase.Handler;
            }
            set {
                _httpContextBase.Handler = value;
            }
        }

        public override bool IsCustomErrorEnabled {
            get {
                return _httpContextBase.IsCustomErrorEnabled;
            }
        }

        public override bool IsDebuggingEnabled {
            get {
                return _httpContextBase.IsDebuggingEnabled;
            }
        }

        public override bool IsPostNotification {
            get {
                return _httpContextBase.IsDebuggingEnabled;
            }
        }

        public override IDictionary Items {
            get {
                return _httpContextBase.Items;
            }
        }

        public override IHttpHandler PreviousHandler {
            get {
                return _httpContextBase.PreviousHandler;
            }
        }

        public override ProfileBase Profile {
            get {
                return _httpContextBase.Profile;
            }
        }

        public override HttpRequestBase Request {
            get {
                return _httpContextBase.Request;
            }
        }

        public override HttpResponseBase Response {
            get {
                return _httpContextBase.Response;
            }
        }

        public override HttpServerUtilityBase Server {
            get {
                return _httpContextBase.Server;
            }
        }

        public override HttpSessionStateBase Session {
            get {
                return _httpContextBase.Session;
            }
        }

        public override bool SkipAuthorization {
            get {
                return _httpContextBase.SkipAuthorization;
            }
            set {
                _httpContextBase.SkipAuthorization = value;
            }
        }

        public override DateTime Timestamp {
            get {
                return _httpContextBase.Timestamp;
            }
        }

        public override TraceContext Trace {
            get {
                return _httpContextBase.Trace;
            }
        }

        public override IPrincipal User {
            get {
                return _httpContextBase.User;
            }
            set {
                _httpContextBase.User = value;
            }
        }


    }
}