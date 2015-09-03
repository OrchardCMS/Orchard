using System;
using System.Threading;

namespace Orchard.Tasks.Locking.Services {

    public class DistributedLock : IDistributedLock {

        private DistributedLockService _service;
        private string _name;
        private int _count;

        public string Name {
            get {
                return _name;
            }
        }

        public DistributedLock(DistributedLockService service, string name) {
            _service = service;
            _name = name;
            _count = 1;
        }

        public void IncreaseReferenceCount() {
            _count++;
        }

        public void Dispose() {
            _count--;
            if (_count == 0) {
                Monitor.Exit(String.Intern(_name));
                _service.ReleaseDistributedLock(this);
            }
        }
    }
}