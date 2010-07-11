using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Compilers {
    public class ProjectFileDescriptor {
        public string AssemblyName { get; set; }
        public IEnumerable<string> SourceFilenames { get; set; }
        public IEnumerable<ReferenceDescriptor> References { get; set; }
    }
    public class ReferenceDescriptor {
        public string AssemblyName { get; set; }
    }
}