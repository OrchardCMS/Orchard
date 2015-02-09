using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Orchard.Utility {
    /// <summary>
    /// Provides locking similar to <see cref="ReaderWriterLockSlim"/> but
    /// </summary>
    /// <remarks>
    /// Taken from http://johnculviner.com/achieving-named-lock-locker-functionality-in-c-4-0/ and adapted a bit. Namely:
    /// - <see cref="GetOrAdd"/> uses a new ReaderWriterLockSlim to overcome possible concurrency issues where the factory delegate could run multiple times.
    /// - Implemented <see cref="IDisposable"/>.
    /// </remarks>
    public class NamedReaderWriterLock : IDisposable {
        private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _lockDictonary = new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        public ReaderWriterLockSlim GetLock(string name) {
            return _lockDictonary.GetOrAdd(name, new ReaderWriterLockSlim());
        }

        public TResult RunWithReadLock<TResult>(string name, Func<TResult> body) {
            var rwLock = GetLock(name);
            try {
                rwLock.EnterReadLock();
                return body();
            }
            finally {
                rwLock.ExitReadLock();
            }
        }

        public void RunWithReadLock(string name, Action body) {
            var rwLock = GetLock(name);
            try {
                rwLock.EnterReadLock();
                body();
            }
            finally {
                rwLock.ExitReadLock();
            }
        }

        public TResult RunWithWriteLock<TResult>(string name, Func<TResult> body) {
            var rwLock = GetLock(name);
            try {
                rwLock.EnterWriteLock();
                return body();
            }
            finally {
                rwLock.ExitWriteLock();
            }
        }

        public void RunWithWriteLock(string name, Action body) {
            var rwLock = GetLock(name);
            try {
                rwLock.EnterWriteLock();
                body();
            }
            finally {
                rwLock.ExitWriteLock();
            }
        }

        public void RemoveLock(string name) {
            ReaderWriterLockSlim o;
            _lockDictonary.TryRemove(name, out o);
        }

        /// <summary>
        /// Disposes all the internal <see cref="ReaderWriterLockSlim"/> objects. Only call this if you're sure that no concurrent code executes
        /// any other instance method of this class!
        /// </summary>
        public void Dispose() {
            foreach (var lockSlim in _lockDictonary.Values) {
                lockSlim.Dispose();
            }
        }
    }
}
