using System.IO;

namespace Orchard.Environment.Compilation.Compilers {
    public interface IProjectFileParser {
        ProjectFileDescriptor Parse(string virtualPath);
        ProjectFileDescriptor Parse(Stream stream);
    }
}