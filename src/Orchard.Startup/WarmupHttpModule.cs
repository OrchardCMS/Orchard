using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Orchard.WarmupStarter {
    public class WarmupHttpModule : IHttpModule {
        private const string WarmupFilesPath = "~/App_Data/Warmup/";
        private HttpApplication _context;

        public void Init(HttpApplication context) {
            _context = context;
            context.AddOnBeginRequestAsync(BeginBeginRequest, EndBeginRequest, null);
        }

        static IList<Action> _awaiting = new List<Action>();

        public static bool InWarmup() {
            if (_awaiting == null) return false;
            lock (_awaiting) {
                return _awaiting != null;
            }
        }

        public static void Signal() {
            lock(typeof(WarmupHttpModule)) {
                if (_awaiting == null) {
                    return;
                }

                var awaiting = _awaiting;
                _awaiting = null;
                foreach (var action in awaiting) {
                    action();
                }
            }
        }

        public static void Await(Action action) {
            if (_awaiting == null) {
                action();
                return;
            }

            lock(typeof(WarmupHttpModule)) {
                if (_awaiting == null) {
                    action();
                    return;
                }
                _awaiting.Add(action);
            }
        }

        private IAsyncResult BeginBeginRequest(object sender, EventArgs e, AsyncCallback cb, object extradata) {
            var asyncResult = new WarmupAsyncResult(cb);
            
            // host is available, process every requests, or file is processed
            if (!InWarmup() || DoBeginRequest()) {
                asyncResult.Done();
            }
            else {
                // this is the "on hold" execution path
                Await(asyncResult.Done);
            }
            
            return asyncResult;
        }

        private static void EndBeginRequest(IAsyncResult ar) {
            ((WarmupAsyncResult)ar).Wait();
        }

        public void Dispose() {
        }

        /// <summary>
        /// return true to put request on hold (until call to Signal()) - return false to allow pipeline to execute immediately
        /// </summary>
        /// <returns></returns>
        private bool DoBeginRequest() {
            // use the url as it was requested by the client
            // the real url might be different if it has been translated (proxy, load balancing, ...)
            var url = ToUrlString(_context.Request);
            var virtualFileCopy = EncodeUrl(url.Trim('/'));
            var localCopy = Path.Combine(HostingEnvironment.MapPath(WarmupFilesPath), virtualFileCopy);

            if (File.Exists(localCopy)) {
                // result should not be cached, even on proxies
                _context.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                _context.Response.Cache.SetValidUntilExpires(false);
                _context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                _context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                _context.Response.Cache.SetNoStore();

                _context.Response.WriteFile(localCopy);
                _context.Response.End();
                return true;
            }

            // there is no local copy and the file exists
            // serve the static file
            if (File.Exists(_context.Request.PhysicalPath)) {
                return true;
            }

            return false;
        }

        public static string EncodeUrl(string url) {
            if(String.IsNullOrWhiteSpace(url)) {
                throw new ArgumentException("url can't be empty");
            }

            var sb = new StringBuilder();
            foreach(var c in url.ToLowerInvariant()) {
                // only accept alphanumeric chars
                if((c >= 'a' && c <= 'z')  || (c >= '0' && c <= '9')) {
                    sb.Append(c);
                }
                // otherwise encode them in UTF8
                else {
                    sb.Append("_");
                    foreach(var b in Encoding.UTF8.GetBytes(new [] {c})) {
                        sb.Append(b.ToString("X"));
                    }
                }
            }

            return sb.ToString();
        }

        public static string ToUrlString(HttpRequest request) {
            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"], request.RawUrl);
        }

        private class WarmupAsyncResult : IAsyncResult {
            readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);

            private readonly AsyncCallback _cb;

            public WarmupAsyncResult(AsyncCallback cb) {
                _cb = cb;
                IsCompleted = false;
            }

            public void Done() {
                IsCompleted = true;
                _eventWaitHandle.Set();
                _cb(this);
            }

            public object AsyncState {
                get { return null; }
            }

            public WaitHandle AsyncWaitHandle {
                get { throw new NotImplementedException(); }
            }

            public bool CompletedSynchronously {
                get { return true; }
            }

            public bool IsCompleted { get; private set; }

            public void Wait() {
                _eventWaitHandle.WaitOne();
            }
        }
    }
}