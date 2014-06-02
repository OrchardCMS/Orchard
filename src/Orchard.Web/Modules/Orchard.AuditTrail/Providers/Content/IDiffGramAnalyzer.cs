using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.AuditTrail.Providers.Content {
    public interface IDiffGramAnalyzer : IDependency {
        /// <summary>
        /// Compares the specified XML elements and returns a DiffGram XML element.
        /// </summary>
        XElement GenerateDiffGram(XElement element1, XElement element2);

        /// <summary>
        /// Analyzes the specified DiffGram against the specified original XML element and returns a list of diff nodes,
        /// where each node describes the difference between the original and updated document.
        /// </summary>
        IEnumerable<DiffNode> Analyze(XElement original, XElement diffGram);
    }
}