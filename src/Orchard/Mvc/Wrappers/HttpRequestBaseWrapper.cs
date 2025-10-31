using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace Orchard.Mvc.Wrappers
{
    public abstract class HttpRequestBaseWrapper : HttpRequestBase
    {
        protected readonly HttpRequestBase _httpRequestBase;

        protected HttpRequestBaseWrapper(HttpRequestBase httpRequestBase)
        {
            _httpRequestBase = httpRequestBase;
        }

        public override byte[] BinaryRead(int count)
        {
            return _httpRequestBase.BinaryRead(count);
        }

        public override int[] MapImageCoordinates(string imageFieldName)
        {
            return _httpRequestBase.MapImageCoordinates(imageFieldName);
        }

        public override string MapPath(string virtualPath)
        {
            return _httpRequestBase.MapPath(virtualPath);
        }

        public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
        {
            return _httpRequestBase.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
        }

        public override void SaveAs(string filename, bool includeHeaders)
        {
            _httpRequestBase.SaveAs(filename, includeHeaders);
        }

        public override void ValidateInput()
        {
            _httpRequestBase.ValidateInput();
        }

        public override string[] AcceptTypes => _httpRequestBase.AcceptTypes;

        public override string AnonymousID => _httpRequestBase.AnonymousID;

        public override string ApplicationPath => _httpRequestBase.ApplicationPath;

        public override string AppRelativeCurrentExecutionFilePath => _httpRequestBase.AppRelativeCurrentExecutionFilePath;

        public override HttpBrowserCapabilitiesBase Browser => _httpRequestBase.Browser;

        public override HttpClientCertificate ClientCertificate => _httpRequestBase.ClientCertificate;

        public override Encoding ContentEncoding
        {
            get
            {
                return _httpRequestBase.ContentEncoding;
            }
            set
            {
                _httpRequestBase.ContentEncoding = value;
            }
        }

        public override int ContentLength => _httpRequestBase.ContentLength;

        public override string ContentType
        {
            get
            {
                return _httpRequestBase.ContentType;
            }
            set
            {
                _httpRequestBase.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies => _httpRequestBase.Cookies;

        public override string CurrentExecutionFilePath => _httpRequestBase.CurrentExecutionFilePath;

        public override string FilePath => _httpRequestBase.FilePath;

        public override HttpFileCollectionBase Files => _httpRequestBase.Files;

        public override Stream Filter
        {
            get
            {
                return _httpRequestBase.Filter;
            }
            set
            {
                _httpRequestBase.Filter = value;
            }
        }

        public override NameValueCollection Form => _httpRequestBase.Form;

        public override NameValueCollection Headers => _httpRequestBase.Headers;

        public override string HttpMethod => _httpRequestBase.HttpMethod;

        public override Stream InputStream => _httpRequestBase.InputStream;

        public override bool IsAuthenticated => _httpRequestBase.IsAuthenticated;

        public override bool IsLocal => _httpRequestBase.IsLocal;

        public override bool IsSecureConnection => _httpRequestBase.IsSecureConnection;

        public override string this[string key]
        {
            get
            {
                return _httpRequestBase[key];
            }
        }

        public override WindowsIdentity LogonUserIdentity => _httpRequestBase.LogonUserIdentity;

        public override NameValueCollection Params => _httpRequestBase.Params;

        public override string Path => _httpRequestBase.Path;

        public override string PathInfo => _httpRequestBase.PathInfo;

        public override string PhysicalApplicationPath => _httpRequestBase.PhysicalApplicationPath;

        public override string PhysicalPath => _httpRequestBase.PhysicalPath;

        public override NameValueCollection QueryString => _httpRequestBase.QueryString;

        public override string RawUrl => _httpRequestBase.RawUrl;

        public override string RequestType
        {
            get
            {
                return _httpRequestBase.RequestType;
            }
            set
            {
                _httpRequestBase.RequestType = value;
            }
        }

        public override NameValueCollection ServerVariables => _httpRequestBase.ServerVariables;

        public override int TotalBytes => _httpRequestBase.TotalBytes;

        public override Uri Url => _httpRequestBase.Url;

        public override Uri UrlReferrer => _httpRequestBase.UrlReferrer;

        public override string UserAgent => _httpRequestBase.UserAgent;

        public override string UserHostAddress => _httpRequestBase.UserHostAddress;

        public override string UserHostName => _httpRequestBase.UserHostName;

        public override string[] UserLanguages => _httpRequestBase.UserLanguages;

    }
}