using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;

namespace Orchard.WarmupStarter {
    public class WarmupHttpModule : IHttpModule {
        private HttpApplication _context;
        private static object _synLock = new object();
        private static IList<Action> _awaiting = new List<Action>();

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
            if (!InWarmup() || WarmupUtility.DoBeginRequest(_context)) {
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