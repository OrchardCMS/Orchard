using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Compilers {
    public enum ReferenceType {
        Library,
        Project
    }

    public class ProjectFileDescriptor {
        public string AssemblyName { get; set; }
        public IEnumerable<string> SourceFilenames { get; set; }
        public IEnumerable<ReferenceDescriptor> References { get; set; }
    }
    public class ReferenceDescriptor {
        public string SimpleName { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public ReferenceType ReferenceType { get; set; }
    }
}