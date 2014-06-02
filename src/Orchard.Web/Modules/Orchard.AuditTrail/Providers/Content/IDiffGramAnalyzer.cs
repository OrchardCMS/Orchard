using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.AuditTrail.Providers.Content {
    public interface IDiffGramAnalyzer : IDependency {
        XElement GenerateDiffGram(XElement element1, XElement element2);
        IEnumerable<DiffNode> Analyze(XElement original, XElement diffGram);
    }
}