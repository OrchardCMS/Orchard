using System;

namespace Orchard.FileSystems.LockFile
{
    public interface ILockFile : IDisposable {
        void Release();
    }
}
