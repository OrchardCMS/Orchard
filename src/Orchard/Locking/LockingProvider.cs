using System;
using System.Threading;

namespace Orchard.Locking {
    public class LockingProvider : ILockingProvider {

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
                innerHandler?.Invoke(ex);
            }
            finally {
                if (taken) {
                    Monitor.Exit(tmp);
                }
            }

            if (outerException != null) {
                outerHandler?.Invoke(outerException);
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
                    innerHandler?.Invoke(ex);
                }
                finally {
                    Monitor.Exit(tmp);
                }

                if (outerException != null) {
                    outerHandler?.Invoke(outerException);
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
                    innerHandler?.Invoke(ex);
                }
                finally {
                    Monitor.Exit(tmp);
                }

                if (outerException != null) {
                    outerHandler?.Invoke(outerException);
                }

                return true;
            }

            return false;
        }


    }
}
