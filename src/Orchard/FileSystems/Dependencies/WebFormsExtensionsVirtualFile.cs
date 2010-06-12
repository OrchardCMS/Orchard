using System.IO;
using System.Web.Hosting;
using Orchard.Environment.Extensions.Loaders;

namespace Orchard.FileSystems.Dependencies {
    public class WebFormsExtensionsVirtualFile : VirtualFile {
        private readonly DependencyDescriptor _dependencyDescriptor;
        private readonly VirtualFile _actualFile;

        public WebFormsExtensionsVirtualFile(string virtualPath, DependencyDescriptor dependencyDescriptor, VirtualFile actualFile)
            : base(virtualPath) {
            _dependencyDescriptor = dependencyDescriptor;
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
            using (var actualStream = _actualFile.Open()) {
                var reader = new StreamReader(actualStream);

                var memoryStream = new MemoryStream();
                int length;
                using (var writer = new StreamWriter(memoryStream)) {

                    bool assemblyDirectiveAdded = false;
                    for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {

                        if (!string.IsNullOrWhiteSpace(line) && !assemblyDirectiveAdded) {
                            line += GetAssemblyDirective();
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

        private string GetAssemblyDirective() {
            if (_dependencyDescriptor.LoaderName == typeof(DynamicExtensionLoader).FullName) {
                return string.Format("<%@ Assembly Src=\"{0}\"%>", _dependencyDescriptor.VirtualPath);
            }
            else {
                return string.Format("<%@ Assembly Name=\"{0}\"%>", _dependencyDescriptor.ModuleName);
            }
        }
    }
}
