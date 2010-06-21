using System.IO;

namespace Orchard.Environment.Extensions.Compilers {
    public interface IProjectFileParser {
        ProjectFileDescriptor Parse(Stream stream);
    }
}