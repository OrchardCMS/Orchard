using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace Orchard.Mvc.Wrappers {
    public abstract class HttpRequestBaseWrapper : HttpRequestBase {
        protected readonly HttpRequestBase _httpRequestBase;

        protected HttpRequestBaseWrapper(HttpRequestBase httpRequestBase) {
            _httpRequestBase = httpRequestBase;
        }

        public override byte[] BinaryRead(int count) {
            return _httpRequestBase.BinaryRead(count);
        }

        public override int[] MapImageCoordinates(string imageFieldName) {
            return _httpRequestBase.MapImageCoordinates(imageFieldName);
        }

        public override string MapPath(string virtualPath) {
            return _httpRequestBase.MapPath(virtualPath);
        }

        public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
            return _httpRequestBase.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
        }

        public override void SaveAs(string filename, bool includeHeaders) {
            _httpRequestBase.SaveAs(filename, includeHeaders);
        }

        public override void ValidateInput() {
            _httpRequestBase.ValidateInput();
        }

        public override string[] AcceptTypes {
            get {
                return _httpRequestBase.AcceptTypes;
            }
        }

        public override string AnonymousID {
            get {
                return _httpRequestBase.AnonymousID;
            }
        }

        public override string ApplicationPath {
            get {
                return _httpRequestBase.ApplicationPath;
            }
        }

        public override string AppRelativeCurrentExecutionFilePath {
            get {
                return _httpRequestBase.AppRelativeCurrentExecutionFilePath;
            }
        }

        public override HttpBrowserCapabilitiesBase Browser {
            get {
                return _httpRequestBase.Browser;
            }
        }

        public override HttpClientCertificate ClientCertificate {
            get {
                return _httpRequestBase.ClientCertificate;
            }
        }

        public override Encoding ContentEncoding {
            get {
                return _httpRequestBase.ContentEncoding;
            }
            set {
                _httpRequestBase.ContentEncoding = value;
            }
        }

        public override int ContentLength {
            get {
                return _httpRequestBase.ContentLength;
            }
        }

        public override string ContentType {
            get {
                return _httpRequestBase.ContentType;
            }
            set {
                _httpRequestBase.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies {
            get {
                return _httpRequestBase.Cookies;
            }
        }

        public override string CurrentExecutionFilePath {
            get {
                return _httpRequestBase.CurrentExecutionFilePath;
            }
        }

        public override string FilePath {
            get {
                return _httpRequestBase.FilePath;
            }
        }

        public override HttpFileCollectionBase Files {
            get {
                return _httpRequestBase.Files;
            }
        }

        public override Stream Filter {
            get {
                return _httpRequestBase.Filter;
            }
            set {
                _httpRequestBase.Filter = value;
            }
        }

        public override NameValueCollection Form {
            get {
                return _httpRequestBase.Form;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _httpRequestBase.Headers;
            }
        }

        public override string HttpMethod {
            get {
                return _httpRequestBase.HttpMethod;
            }
        }

        public override Stream InputStream {
            get {
                return _httpRequestBase.InputStream;
            }
        }

        public override bool IsAuthenticated {
            get {
                return _httpRequestBase.IsAuthenticated;
            }
        }

        public override bool IsLocal {
            get {
                return _httpRequestBase.IsLocal;
            }
        }

        public override bool IsSecureConnection {
            get {
                return _httpRequestBase.IsSecureConnection;
            }
        }

        public override string this[string key] {
            get {
                return _httpRequestBase[key];
            }
        }

        public override WindowsIdentity LogonUserIdentity {
            get {
                return _httpRequestBase.LogonUserIdentity;
            }
        }

        public override NameValueCollection Params {
            get {
                return _httpRequestBase.Params;
            }
        }

        public override string Path {
            get {
                return _httpRequestBase.Path;
            }
        }

        public override string PathInfo {
            get {
                return _httpRequestBase.PathInfo;
            }
        }

        public override string PhysicalApplicationPath {
            get {
                return _httpRequestBase.PhysicalApplicationPath;
            }
        }

        public override string PhysicalPath {
            get {
                return _httpRequestBase.PhysicalPath;
            }
        }

        public override NameValueCollection QueryString {
            get {
                return _httpRequestBase.QueryString;
            }
        }

        public override string RawUrl {
            get {
                return _httpRequestBase.RawUrl;
            }
        }

        public override string RequestType {
            get {
                return _httpRequestBase.RequestType;
            }
            set {
                _httpRequestBase.RequestType = value;
            }
        }

        public override NameValueCollection ServerVariables {
            get {
                return _httpRequestBase.ServerVariables;
            }
        }

        public override int TotalBytes {
            get {
                return _httpRequestBase.TotalBytes;
            }
        }

        public override Uri Url {
            get {
                return _httpRequestBase.Url;
            }
        }

        public override Uri UrlReferrer {
            get {
                return _httpRequestBase.UrlReferrer;
            }
        }

        public override string UserAgent {
            get {
                return _httpRequestBase.UserAgent;
            }
        }

        public override string UserHostAddress {
            get {
                return _httpRequestBase.UserHostAddress;
            }
        }

        public override string UserHostName {
            get {
                return _httpRequestBase.UserHostName;
            }
        }

        public override string[] UserLanguages {
            get {
                return _httpRequestBase.UserLanguages;
            }
        }

    }
}