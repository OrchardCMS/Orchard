using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Orchard.Mvc.Wrappers {
    public abstract class HttpResponseBaseWrapper : HttpResponseBase {
        private readonly HttpResponseBase _httpResponseBase;

        protected HttpResponseBaseWrapper(HttpResponseBase httpResponse) {
            _httpResponseBase = httpResponse;
        }

        public override void AddCacheDependency(params CacheDependency[] dependencies) {
            _httpResponseBase.AddCacheDependency(dependencies);
        }

        public override void AddCacheItemDependencies(ArrayList cacheKeys) {
            _httpResponseBase.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependencies(string[] cacheKeys) {
            _httpResponseBase.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependency(string cacheKey) {
            _httpResponseBase.AddCacheItemDependency(cacheKey);
        }

        public override void AddFileDependencies(string[] filenames) {
            _httpResponseBase.AddFileDependencies(filenames);
        }

        public override void AddFileDependencies(ArrayList filenames) {
            _httpResponseBase.AddFileDependencies(filenames);
        }

        public override void AddFileDependency(string filename) {
            _httpResponseBase.AddFileDependency(filename);
        }

        public override void AddHeader(string name, string value) {
            _httpResponseBase.AddHeader(name, value);
        }

        public override void AppendCookie(HttpCookie cookie) {
            _httpResponseBase.AppendCookie(cookie);
        }

        public override void AppendHeader(string name, string value) {
            _httpResponseBase.AppendHeader(name, value);
        }

        public override void AppendToLog(string param) {
            _httpResponseBase.AppendToLog(param);
        }

        public override string ApplyAppPathModifier(string virtualPath) {
            return _httpResponseBase.ApplyAppPathModifier(virtualPath);
        }

        public override void BinaryWrite(byte[] buffer) {
            _httpResponseBase.BinaryWrite(buffer);
        }

        public override void Clear() {
            _httpResponseBase.Clear();
        }

        public override void ClearContent() {
            _httpResponseBase.ClearContent();
        }

        public override void ClearHeaders() {
            _httpResponseBase.ClearHeaders();
        }

        public override void Close() {
            _httpResponseBase.Close();
        }

        public override void DisableKernelCache() {
            _httpResponseBase.DisableKernelCache();
        }

        public override void End() {
            _httpResponseBase.End();
        }

        public override void Flush() {
            _httpResponseBase.Flush();
        }

        public override void Pics(string value) {
            _httpResponseBase.Pics(value);
        }

        public override void Redirect(string url) {
            _httpResponseBase.Redirect(url);
        }

        public override void Redirect(string url, bool endResponse) {
            _httpResponseBase.Redirect(url, endResponse);
        }

        public override void RemoveOutputCacheItem(string path) {
            _httpResponseBase.RemoveOutputCacheItem(path);
        }

        public override void SetCookie(HttpCookie cookie) {
            _httpResponseBase.SetCookie(cookie);
        }

        public override void TransmitFile(string filename) {
            _httpResponseBase.TransmitFile(filename);
        }

        public override void TransmitFile(string filename, long offset, long length) {
            _httpResponseBase.TransmitFile(filename, offset, length);
        }

        public override void Write(char ch) {
            _httpResponseBase.Write(ch);
        }

        public override void Write(object obj) {
            _httpResponseBase.Write(obj);
        }

        public override void Write(string s) {
            _httpResponseBase.Write(s);
        }

        public override void Write(char[] buffer, int index, int count) {
            _httpResponseBase.Write(buffer, index, count);
        }

        public override void WriteFile(string filename) {
            _httpResponseBase.WriteFile(filename);
        }

        public override void WriteFile(string filename, bool readIntoMemory) {
            _httpResponseBase.WriteFile(filename, readIntoMemory);
        }

        public override void WriteFile(IntPtr fileHandle, long offset, long size) {
            _httpResponseBase.WriteFile(fileHandle, offset, size);
        }

        public override void WriteFile(string filename, long offset, long size) {
            _httpResponseBase.WriteFile(filename, offset, size);
        }

        public override void WriteSubstitution(HttpResponseSubstitutionCallback callback) {
            _httpResponseBase.WriteSubstitution(callback);
        }

        // Properties
        public override bool Buffer {
            get {
                return _httpResponseBase.Buffer;
            }
            set {
                _httpResponseBase.Buffer = value;
            }
        }

        public override bool BufferOutput {
            get {
                return _httpResponseBase.BufferOutput;
            }
            set {
                _httpResponseBase.BufferOutput = value;
            }
        }

        public override HttpCachePolicyBase Cache {
            get {
                return _httpResponseBase.Cache;
            }
        }

        public override string CacheControl {
            get {
                return _httpResponseBase.CacheControl;
            }
            set {
                _httpResponseBase.CacheControl = value;
            }
        }

        public override string Charset {
            get {
                return _httpResponseBase.Charset;
            }
            set {
                _httpResponseBase.Charset = value;
            }
        }

        public override Encoding ContentEncoding {
            get {
                return _httpResponseBase.ContentEncoding;
            }
            set {
                _httpResponseBase.ContentEncoding = value;
            }
        }

        public override string ContentType {
            get {
                return _httpResponseBase.ContentType;
            }
            set {
                _httpResponseBase.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies {
            get {
                return _httpResponseBase.Cookies;
            }
        }

        public override int Expires {
            get {
                return _httpResponseBase.Expires;
            }
            set {
                _httpResponseBase.Expires = value;
            }
        }

        public override DateTime ExpiresAbsolute {
            get {
                return _httpResponseBase.ExpiresAbsolute;
            }
            set {
                _httpResponseBase.ExpiresAbsolute = value;
            }
        }

        public override Stream Filter {
            get {
                return _httpResponseBase.Filter;
            }
            set {
                _httpResponseBase.Filter = value;
            }
        }

        public override Encoding HeaderEncoding {
            get {
                return _httpResponseBase.HeaderEncoding;
            }
            set {
                _httpResponseBase.HeaderEncoding = value;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _httpResponseBase.Headers;
            }
        }

        public override bool IsClientConnected {
            get {
                return _httpResponseBase.IsClientConnected;
            }
        }

        public override bool IsRequestBeingRedirected {
            get {
                return _httpResponseBase.IsRequestBeingRedirected;
            }
        }

        public override TextWriter Output {
            get {
                return _httpResponseBase.Output;
            }
        }

        public override Stream OutputStream {
            get {
                return _httpResponseBase.OutputStream;
            }
        }

        public override string RedirectLocation {
            get {
                return _httpResponseBase.RedirectLocation;
            }
            set {
                _httpResponseBase.RedirectLocation = value;
            }
        }

        public override string Status {
            get {
                return _httpResponseBase.Status;
            }
            set {
                _httpResponseBase.Status = value;
            }
        }

        public override int StatusCode {
            get {
                return _httpResponseBase.StatusCode;
            }
            set {
                _httpResponseBase.StatusCode = value;
            }
        }

        public override string StatusDescription {
            get {
                return _httpResponseBase.StatusDescription;
            }
            set {
                _httpResponseBase.StatusDescription = value;
            }
        }

        public override int SubStatusCode {
            get {
                return _httpResponseBase.SubStatusCode;
            }
            set {
                _httpResponseBase.SubStatusCode = value;
            }
        }

        public override bool SuppressContent {
            get {
                return _httpResponseBase.SuppressContent;
            }
            set {
                _httpResponseBase.SuppressContent = value;
            }
        }

        public override bool TrySkipIisCustomErrors {
            get {
                return _httpResponseBase.TrySkipIisCustomErrors;
            }
            set {
                _httpResponseBase.TrySkipIisCustomErrors = value;
            }
        }

    }
}