using System;

namespace Orchard.Environment.Extensions {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class OrchardDependencyReplaceAttribute : Attribute {
        public OrchardDependencyReplaceAttribute(string dependency) {
            Dependency = dependency;
        }

        public string Dependency { get; set; }
    }
}