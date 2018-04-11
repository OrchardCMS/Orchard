using System;
using System.Threading;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Locking {
    public class LockingProvider : ILockingProvider {

        public LockingProvider() {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void Lock(
            object lockOn,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            LockInternal(lockOn, criticalCode, innerHandler, outerHandler);
        }

        public void Lock(
            string lockOn,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            LockInternal(String.Intern(lockOn), criticalCode, innerHandler, outerHandler);
        }

        public bool TryLock(
            object lockOn,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            return TryLockInternal(lockOn, TimeSpan.Zero, criticalCode, innerHandler, outerHandler);
        }

        public bool TryLock(
            string lockOn,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            return TryLockInternal(String.Intern(lockOn), TimeSpan.Zero, criticalCode, innerHandler, outerHandler);
        }

        public bool TryLock(
           object lockOn,
           TimeSpan timeout,
           Action criticalCode,
           Action<Exception> innerHandler = null,
           Action<Exception> outerHandler = null) {

            return TryLockInternal(lockOn, timeout, criticalCode, innerHandler, outerHandler);
        }

        public bool TryLock(
            string lockOn,
            TimeSpan timeout,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            return TryLockInternal(String.Intern(lockOn), timeout, criticalCode, innerHandler, outerHandler);
        }

        public bool TryLock(
            object lockOn,
            int millisecondsTimeout,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            return TryLockInternal(lockOn, millisecondsTimeout, criticalCode, innerHandler, outerHandler);
        }

        public bool TryLock(
            string lockOn,
            int millisecondsTimeout,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            return TryLockInternal(String.Intern(lockOn), millisecondsTimeout, criticalCode, innerHandler, outerHandler);
        }

        private void LockInternal(
            object lockOn,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            bool taken = false;
            var tmp = lockOn;
            Exception outerException = null;
            try {
                Monitor.Enter(tmp, ref taken);
                criticalCode?.Invoke();
            }
            catch (Exception ex) {
                outerException = ex;
                CleanLog(ex);
                if (innerHandler != null) {
                    innerHandler.Invoke(ex);
                }
                else {
                    if (outerHandler == null) {
                        // if both the handlers are null, the methods should behave like lock(tmp){}
                        // and only bubble out the exception while holding the lock.
                        outerException = null;
                    }
                    throw ex;
                }
            }
            finally {
                if (taken) {
                    Monitor.Exit(tmp);
                }
            }

            // Even if there was an handler for the exception to be used in the critical section
            // (i.e. innerHandler != null) we have further handling here. This may simply mean throwing
            // the exception out when outerHandler == null
            if (outerException != null) {
                if (outerHandler != null) {
                    outerHandler.Invoke(outerException);
                }
                else {
                    throw outerException;
                }
            }
        }

        private bool TryLockInternal(
            object lockOn,
            TimeSpan timeout,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            var tmp = lockOn;
            Exception outerException = null;

            if (Monitor.TryEnter(tmp, timeout)) {
                try {
                    criticalCode?.Invoke();
                }
                catch (Exception ex) {
                    outerException = ex;
                    CleanLog(ex);
                    if (innerHandler != null) {
                        innerHandler.Invoke(ex);
                    }
                    else {
                        if (outerHandler == null) {
                            // if both the handlers are null, the methods should behave like lock(tmp){}
                            // and only bubble out the exception while holding the lock.
                            outerException = null;
                        }
                        throw ex;
                    }
                }
                finally {
                    Monitor.Exit(tmp);
                }

                // Even if there was an handler for the exception to be used in the critical section
                // (i.e. innerHandler != null) we have further handling here. This may simply mean throwing
                // the exception out when outerHandler == null
                if (outerException != null) {
                    if (outerHandler != null) {
                        outerHandler.Invoke(outerException);
                    }
                    else {
                        throw outerException;
                    }
                }

                return true;
            }

            return false;
        }

        private bool TryLockInternal(
            object lockOn,
            int millisecondsTimeout,
            Action criticalCode,
            Action<Exception> innerHandler = null,
            Action<Exception> outerHandler = null) {

            var tmp = lockOn;
            Exception outerException = null;

            if (Monitor.TryEnter(tmp, millisecondsTimeout)) {
                try {
                    criticalCode?.Invoke();
                }
                catch (Exception ex) {
                    outerException = ex;
                    CleanLog(ex);
                    if (innerHandler != null) {
                        innerHandler.Invoke(ex);
                    }
                    else {
                        if (outerHandler == null) {
                            // if both the handlers are null, the methods should behave like lock(tmp){}
                            // and only bubble out the exception while holding the lock.
                            outerException = null;
                        }
                        throw ex;
                    }
                }
                finally {
                    Monitor.Exit(tmp);
                }

                // Even if there was an handler for the exception to be used in the critical section
                // (i.e. innerHandler != null) we have further handling here. This may simply mean throwing
                // the exception out when outerHandler == null
                if (outerException != null) {
                    if (outerHandler != null) {
                        outerHandler.Invoke(outerException);
                    }
                    else {
                        throw outerException;
                    }
                }

                return true;
            }

            return false;
        }

        private void CleanLog(Exception ex) {
            try {
                Logger.Log(Logging.LogLevel.Error, ex, T("Exception while running critical code").Text);
            }
            catch (Exception) {
                // prevent messing things up if the logger fails
            }
        }
    }
}
