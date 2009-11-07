using System;
using System.IO;

namespace Orchard.Storage {
    public interface IStorageFile {
        string GetPath();
        string GetName();
        long GetSize();
        DateTime GetLastUpdated();
        string GetFileType();
        Stream OpenStream();
    }
}
