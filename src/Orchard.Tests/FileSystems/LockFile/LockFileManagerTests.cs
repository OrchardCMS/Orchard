using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.LockFile;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.FileSystems.LockFile {
    [TestFixture]
    public class LockFileManagerTests {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;
        private ILockFileManager _lockFileManager;
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
            _lockFileManager = new DefaultLockFileManager(_appDataFolder, _clock);
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        [Test]
        public void LockShouldBeGrantedWhenDoesNotExist() {
            ILockFile lockFile = null;
            var granted = _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            Assert.That(granted, Is.True);
            Assert.That(lockFile, Is.Not.Null);
            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.True);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ExistingLockFileShouldPreventGrants() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);
            
            Assert.That(_lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile), Is.False);
            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.True);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ReleasingALockShouldAllowGranting() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            using (lockFile) {
                Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.True);
                Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
            }

            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.False);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(0));
        }

        [Test]
        public void ReleasingAReleasedLockShouldWork() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);
            
            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.True);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
            
            lockFile.Release();
            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.False);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(0));
            
            lockFile.Release();
            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.False);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(0));
        }

        [Test]
        public void DisposingLockShouldReleaseIt() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            using (lockFile) {
                Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.True);
                Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
            }

            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.False);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(0));
        }

        [Test]
        public void ExpiredLockShouldBeAvailable() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            _clock.Advance(DefaultLockFileManager.Expiration);
            Assert.That(_lockFileManager.IsLocked("foo.txt.lock"), Is.False);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldGrantExpiredLock() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            _clock.Advance(DefaultLockFileManager.Expiration);
            var granted = _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            Assert.That(granted, Is.True);
            Assert.That(_appDataFolder.ListFiles("").Count(), Is.EqualTo(1));
        }

        private static int _lockCount;
        private static readonly object _synLock = new object();

        [Test]
        public void AcquiringLockShouldBeThreadSafe() {
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
            var threads = new List<Thread>();
            for (var i = 0; i < 10; i++)
            {
                var t = new Thread(PlayWithIsLocked);
                t.Start();
                threads.Add(t);
            }

            threads.ForEach(t => t.Join());
            Assert.That(_lockCount, Is.EqualTo(0));
        }

        private void PlayWithAcquire() {
            var r = new Random(DateTime.Now.Millisecond); 
            ILockFile lockFile = null;

            // loop until the lock has been acquired
            for (;;) {
                if (!_lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile)) {
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

                lockFile.Release();
                return;
            }
        }

        private void PlayWithIsLocked() {
            var r = new Random(DateTime.Now.Millisecond); 
            ILockFile lockFile = null;
            const string path = "foo.txt.lock";

            // loop until the lock has been acquired
            for (;;) {
                if(_lockFileManager.IsLocked(path)) {
                    continue;
                }

                if (!_lockFileManager.TryAcquireLock(path, ref lockFile)) {
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

                lockFile.Release();
                return;
            }
        }

    }
}
