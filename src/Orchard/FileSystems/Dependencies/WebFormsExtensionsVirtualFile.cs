using System.IO;
using System.Web.Hosting;

namespace Orchard.FileSystems.Dependencies {
    public class WebFormVirtualFile : VirtualFile {
        private readonly VirtualFile _actualFile;
        private readonly string _assemblyDirective;

        public WebFormVirtualFile(string virtualPath, VirtualFile actualFile, string assemblyDirective)
            : base(virtualPath) {
            _actualFile = actualFile;
            _assemblyDirective = assemblyDirective;
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
            using (var actualStream = _actualFile.Open()) {
                var reader = new StreamReader(actualStream);

                var memoryStream = new MemoryStream();
                int length;
                using (var writer = new StreamWriter(memoryStream)) {

                    bool assemblyDirectiveAdded = false;
                    for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {

                        if (!string.IsNullOrWhiteSpace(line) && !assemblyDirectiveAdded) {
                            line += _assemblyDirective;
                            assemblyDirectiveAdded = true;
                        }

                        writer.WriteLine(line);
                    }
                    writer.Flush();
                    length = (int) memoryStream.Length;
                }
                return new MemoryStream(memoryStream.GetBuffer(), 0, length);
            }
        }
    }
}
