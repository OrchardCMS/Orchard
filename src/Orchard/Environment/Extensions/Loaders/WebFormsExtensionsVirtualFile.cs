using System.IO;
using System.Reflection;
using System.Web.Hosting;

namespace Orchard.Environment.Extensions.Loaders {
    public class WebFormsExtensionsVirtualFile : VirtualFile {
        private readonly Assembly _assembly;
        private readonly VirtualFile _actualFile;

        public WebFormsExtensionsVirtualFile(string virtualPath, Assembly assembly, VirtualFile actualFile)
            : base(virtualPath) {
            _assembly = assembly;
            _actualFile = actualFile;
        }

        public override string Name {
            get {
                return _actualFile.Name;
            }
        }

        public override bool IsDirectory {
            get {
                return _actualFile.IsDirectory;
            }
        }

        public override Stream Open() {
            var reader = new StreamReader(_actualFile.Open());
            var memoryStream = new MemoryStream();
            int length;
            using (var writer = new StreamWriter(memoryStream)) {

                bool assemblyDirectiveAdded = false;
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {

                    if (!string.IsNullOrWhiteSpace(line) && !assemblyDirectiveAdded) {
                        line += string.Format("<%@ Assembly Name=\"{0}\"%>", _assembly);
                        assemblyDirectiveAdded = true;
                    }

                    writer.WriteLine(line);
                }
                writer.Flush();
                length = (int)memoryStream.Length;
            }
            var result = new MemoryStream(memoryStream.GetBuffer(), 0, length);
            return result;
        }
    }
}
