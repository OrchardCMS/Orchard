using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Orchard.WarmupStarter {
    public class WarmupHttpModule : IHttpModule {
        private const string WarmupFilesPath = "~/App_Data/Warmup/";
        private HttpApplication _context;
        private static object _synLock = new object();
        private static IList<Action> _awaiting = new List<Action>();

        public WarmupHttpModule() {
        }

        public void Init(HttpApplication context) {
            _context = context;
            context.AddOnBeginRequestAsync(BeginBeginRequest, EndBeginRequest, null);
        }

        public void Dispose() {
        }

        private static bool InWarmup() {
            lock (_synLock) {
                return _awaiting != null;
            }
        }

        /// <summary>
        /// Called to unblock all pending requests, while remaining in "queue" incoming requests mode.
        /// New incoming request are queued in the "_await" list.
        /// </summary>
        public static void ProcessPendingRequests() {
            FlushAwaitingRequests(new List<Action>());
        }

        /// <summary>
        /// Pending requests in the "_await" queue are processed, and any new incoming request
        /// is now processed immediately.
        /// </summary>
        public static void SignalWarmupDone() {
            FlushAwaitingRequests(null);
        }

        public static void SignalWarmupStart() {
            lock (_synLock) {
                if (_awaiting == null) {
                    _awaiting = new List<Action>();
                }
            }
        }

        private static void FlushAwaitingRequests(IList<Action> newAwaiting) {
            IList<Action> temp;

            lock (_synLock) {
                if (_awaiting == null) {
                    return;
                }

                temp = _awaiting;
                _awaiting = newAwaiting;
            }

            foreach (var action in temp) {
                    action();
                }
            }

        /// <summary>
        /// Enqueue or directly process action depending on current mode.
        /// </summary>
        private void Await(Action action) {
            Action temp = action;

            lock (_synLock) {
                if (_awaiting != null) {
                    temp = null;
                    _awaiting.Add(action);
                }
        }

            if (temp != null) {
                temp();
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

        /// <summary>
        /// return true to put request on hold (until call to Signal()) - return false to allow pipeline to execute immediately
        /// </summary>
        /// <returns></returns>
        private bool DoBeginRequest() {
            // use the url as it was requested by the client
            // the real url might be different if it has been translated (proxy, load balancing, ...)
            var url = ToUrlString(_context.Request);
            var virtualFileCopy = WarmupUtility.EncodeUrl(url.Trim('/'));
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

        public static string ToUrlString(HttpRequest request) {
            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"], request.RawUrl);
        }

        private class WarmupAsyncResult : IAsyncResult {
            private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false);
            private readonly AsyncCallback _cb;
            private bool _isCompleted;

            public WarmupAsyncResult(AsyncCallback cb) {
                _cb = cb;
                _isCompleted = false;
            }

            public void Done() {
                _isCompleted = true;
                _eventWaitHandle.Set();
                _cb(this);
            }

            public void Wait() {
                _eventWaitHandle.WaitOne();
            }

            object IAsyncResult.AsyncState {
                get { return null; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle {
                get { throw new NotImplementedException(); }
            }

            bool IAsyncResult.CompletedSynchronously {
                get { return false; }
            }

            bool IAsyncResult.IsCompleted {
                get { return _isCompleted; }
            }
        }
    }
}