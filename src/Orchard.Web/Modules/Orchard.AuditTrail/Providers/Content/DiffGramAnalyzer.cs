using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.XmlDiffPatch;

namespace Orchard.AuditTrail.Providers.Content {
    public class DiffGramAnalyzer : IDiffGramAnalyzer {
        public XElement GenerateDiffGram(XElement element1, XElement element2) {
            using(var node1Reader = element1.CreateReader())
            using (var node2Reader = element2.CreateReader()) {
                var result = new XDocument();
                using (var writer = result.CreateWriter()) {
                    var diff =
                        new XmlDiff(XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace |
                                    XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl);
                    diff.Compare(node1Reader, node2Reader, writer);
                    writer.Flush();
                    writer.Close();
                }

                return result.Root;
            }
        }

        public IEnumerable<DiffNode> Analyze(XElement original, XElement diffGram) {
            var stack = new Stack<XElement>();

            stack.Push(new XElement("original", original));

            using (var reader = diffGram.CreateReader()) {
                while (!reader.EOF) {
                    var doRead = true;
                    if (reader.LocalName == "xmldiff")
                        reader.Read();
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            var match = reader.GetAttribute("match");
                            var isAttributeChange = match != null && match.StartsWith("@");
                            var index = match == null || isAttributeChange ? default(int?) : Int32.Parse(match) - 1;
                            var diffType = reader.LocalName;
                            var currentElement = stack.Peek();

                            if (currentElement.HasElements && index != null) {
                                var sourceElement = currentElement.Elements().ElementAt(index.Value);
                                stack.Push(sourceElement);
                            }

                            if (diffType != "node") {
                                switch (diffType) {
                                    case "change":
                                        if (isAttributeChange) {
                                            var attributeName = match.Substring(1);
                                            var originalValue = currentElement.Attribute(attributeName).Value;
                                            var currentValue = reader.ReadElementContentAsString();
                                            doRead = false;
                                            yield return
                                                new DiffNode {
                                                    Type = DiffType.Change,
                                                    ElementName = attributeName,
                                                    Previous = originalValue,
                                                    Current = currentValue
                                                };
                                        }
                                        else {
                                            var originalContent = currentElement.Value;
                                            var currentContent = reader.ReadElementContentAsString();
                                            stack.Pop();
                                            doRead = false;
                                            yield return
                                                new DiffNode {
                                                    Type = DiffType.Change,
                                                    ElementName = currentElement.Name.ToString(),
                                                    Previous = originalContent,
                                                    Current = currentContent
                                                };
                                        }
                                        break;
                                    case "add":
                                        reader.Read();
                                        var addedElementContent = reader.ReadElementContentAsString();
                                        yield return new DiffNode { Type = DiffType.Addition, ElementName = reader.Name, Current = addedElementContent };
                                        break;
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            stack.Pop();
                            break;
                    }
                    if (doRead)
                        reader.Read();
                }
            }
        }
    }
}