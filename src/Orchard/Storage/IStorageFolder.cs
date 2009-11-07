using System;

namespace Orchard.Storage {
    public interface IStorageFolder {
        string GetName();
        long GetSize();
        DateTime GetLastUpdated();
        IStorageFolder GetParent();
    }
}
