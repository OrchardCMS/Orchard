using System;

namespace Orchard.Tasks.Locking.Services
{

    public class DistributedLock : IDistributedLock
    {

        private readonly string _name;
        private readonly Action _releaseLockAction;
        private int _count;

        internal DistributedLock(string name, string internalName, Action releaseLockAction)
        {
            _name = name;
            InternalName = internalName;
            _releaseLockAction = releaseLockAction;
            _count = 1;
        }

        string IDistributedLock.Name => _name;

        internal string InternalName { get; }

        internal void Increment()
        {
            _count++;
        }

        public void Dispose()
        {
            _count--;
            if (_count == 0)
                _releaseLockAction();
        }
    }
}