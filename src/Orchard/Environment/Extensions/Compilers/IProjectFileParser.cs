using System.IO;

namespace Orchard.Environment.Extensions.Compilers {
    public interface IProjectFileParser {
        ProjectFileDescriptor Parse(string virtualPath);
        ProjectFileDescriptor Parse(Stream stream);
    }
}