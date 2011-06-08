using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Locking;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Locking {
    [TestFixture]
    public class LockManagerTests {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;
        private ILockManager _lockFileManager;
        private StubClock _clock;

        public class StubAppDataFolderRoot : IAppDataFolderRoot {
            public string RootPath { get; set; }
            public string RootFolder { get; set; }
        }

        public static IAppDataFolder CreateAppDataFolder(string tempFolder) {
            var folderRoot = new StubAppDataFolderRoot {RootPath = "~/App_Data", RootFolder = tempFolder};
            var monitor = new StubVirtualPathMonitor();
            return new AppDataFolder(folderRoot, monitor);
        }

        [SetUp]
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            _appDataFolder = CreateAppDataFolder(_tempFolder);

            _clock = new StubClock();
            _lockFileManager = new DefaultLockManager(_appDataFolder, _clock, new ShellSettings { Name = "Foo" });
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        private int LockFilesCount() {
            return _appDataFolder.ListFiles("Sites/Foo").Where(f => f.EndsWith(".lock")).Count();
        }

        [Test]
        public void LockShouldBeGrantedWhenDoesNotExist() {
            var @lock = _lockFileManager.Lock("foo.txt");

            Assert.That(@lock, Is.Not.Null);
            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.True);
            Assert.That(LockFilesCount(), Is.EqualTo(1));
        }

        [Test]
        public void ExistingLockFileShouldPreventGrants() {
            _lockFileManager.Lock("foo.txt");

            Assert.That(_lockFileManager.Lock("foo.txt"), Is.Null);
            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.True);
            Assert.That(LockFilesCount(), Is.EqualTo(1));
        }

        [Test]
        public void ReleasingALockShouldAllowGranting() {
            var @lock = _lockFileManager.Lock("foo.txt");
            
            using (@lock) {
                Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.True);
                Assert.That(LockFilesCount(), Is.EqualTo(1));
            }

            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.False);
            Assert.That(LockFilesCount(), Is.EqualTo(0));
        }

        [Test]
        public void ReleasingAReleasedLockShouldWork() {
            var @lock = _lockFileManager.Lock("foo.txt");

            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.True);
            Assert.That(LockFilesCount(), Is.EqualTo(1));

            @lock.Dispose();
            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.False);
            Assert.That(LockFilesCount(), Is.EqualTo(0));

            @lock.Dispose();
            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.False);
            Assert.That(LockFilesCount(), Is.EqualTo(0));
        }

        [Test]
        public void DisposingLockShouldReleaseIt() {
            var @lock = _lockFileManager.Lock("foo.txt");

            using (@lock) {
                Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.True);
                Assert.That(LockFilesCount(), Is.EqualTo(1));
            }

            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.False);
            Assert.That(LockFilesCount(), Is.EqualTo(0));
        }

        [Test]
        public void ExpiredLockShouldBeAvailable() {
            _lockFileManager.Lock("foo.txt");

            _clock.Advance(DefaultLockManager.Expiration);
            Assert.That(_lockFileManager.IsLocked("foo.txt"), Is.False);
            Assert.That(LockFilesCount(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldGrantExpiredLock() {
            _lockFileManager.Lock("foo.txt");

            _clock.Advance(DefaultLockManager.Expiration);
            var @lock = _lockFileManager.Lock("foo.txt");

            Assert.That(@lock, Is.Not.Null);
            Assert.That(LockFilesCount(), Is.EqualTo(1));
        }

        private static int _lockCount;
        private static readonly object _synLock = new object();

        [Test]
        public void AcquiringLockShouldBeThreadSafe() {
            // A number of threads will try to acquire a lock and keep it for 
            // some random time. Each of them stops when it has acquired the lock once.

            var threads = new List<Thread>();
            for(var i=0; i<10; i++) {
                var t = new Thread(PlayWithAcquire);
                t.Start();
                threads.Add(t);
            }

            threads.ForEach(t => t.Join());
            Assert.That(_lockCount, Is.EqualTo(0));
        }

        [Test]
        public void IsLockedShouldBeThreadSafe() {
            // A number of threads will try to acquire a lock and keep it for 
            // some random time. Each of them stops when it has acquired the lock once.

            var threads = new List<Thread>();
            for (var i = 0; i < 10; i++) {
                var t = new Thread(PlayWithIsLocked);
                t.Start();
                threads.Add(t);
            }

            threads.ForEach(t => t.Join());
            Assert.That(_lockCount, Is.EqualTo(0));
        }

        private void PlayWithAcquire() {
            var r = new Random(DateTime.Now.Millisecond);
            IDisposable @lock;

            // loop until the lock has been acquired
            for (;;) {
                if (null == (@lock = _lockFileManager.Lock("foo.txt"))) {
                    continue;
                }

                lock (_synLock) {
                    _lockCount++;
                    Assert.That(_lockCount, Is.EqualTo(1));
                }

                // keep the lock for a certain time
                Thread.Sleep(r.Next(200));
                lock (_synLock) {
                    _lockCount--;
                    Assert.That(_lockCount, Is.EqualTo(0));
                }

                @lock.Dispose();
                return;
            }
        }

        private void PlayWithIsLocked() {
            var r = new Random(DateTime.Now.Millisecond);
            IDisposable @lock;
            const string path = "foo.txt";

            // loop until the lock has been acquired
            for (;;) {
                if (_lockFileManager.IsLocked(path)) {
                    continue;
                }

                if (null == (@lock = _lockFileManager.Lock(path))) {
                    continue;
                }

                lock (_synLock) {
                    _lockCount++;
                    Assert.That(_lockCount, Is.EqualTo(1));
                }

                // keep the lock for a certain time
                Thread.Sleep(r.Next(200));
                lock (_synLock) {
                    _lockCount--;
                    Assert.That(_lockCount, Is.EqualTo(0));
                }

                @lock.Dispose();
                return;
            }
        }
    }
}
