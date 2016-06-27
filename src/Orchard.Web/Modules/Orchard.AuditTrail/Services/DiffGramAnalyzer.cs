using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.XmlDiffPatch;
using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.Services {
    public class DiffGramAnalyzer : IDiffGramAnalyzer {
        public XElement GenerateDiffGram(XElement element1, XElement element2) {
            using (var node1Reader = element1.CreateReader())
            using (var node2Reader = element2.CreateReader()) {
                var result = new XDocument();
                using (var writer = result.CreateWriter()) {
                    var diff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace | XmlDiffOptions.IgnoreComments | XmlDiffOptions.IgnoreXmlDecl);
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
                    var readNext = true;
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

                                            readNext = false;
                                            yield return
                                                new DiffNode {
                                                    Type = DiffType.Change,
                                                    Context = BuildContextName(stack, attributeName),
                                                    Previous = originalValue,
                                                    Current = currentValue
                                                };
                                        }
                                        else {
                                            var elementName = currentElement.Name.ToString();
                                            var originalContent = currentElement.Value;
                                            var currentContent = reader.ReadElementContentAsString();

                                            readNext = false;
                                            yield return
                                                new DiffNode {
                                                    Type = DiffType.Change,
                                                    Context = BuildContextName(stack, elementName),
                                                    Previous = originalContent,
                                                    Current = currentContent
                                                };
                                        }
                                        break;
                                    case "add":
                                        var nodeName = reader.GetAttribute("name");
                                        var addedContent = default(string);
                                        reader.Read();
                                        if (reader.NodeType != XmlNodeType.EndElement) {
                                            nodeName = reader.Name;
                                            addedContent = reader.ReadOuterXml();
                                        }
                                        yield return 
                                            new DiffNode { 
                                                Type = DiffType.Addition, 
                                                Context = BuildContextName(stack, nodeName), 
                                                Current = addedContent 
                                            };
                                        break;
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if(stack.Any())
                                stack.Pop();
                            break;
                    }
                    if (readNext)
                        reader.Read();
                }
            }
        }

        private string BuildContextName(IEnumerable<XElement> stack, string nodeName) {
            return String.Format("{0}/{1}", String.Join("/", stack.Reverse().Skip(1).Select(x => x.Name)), nodeName);
        }
    }
}