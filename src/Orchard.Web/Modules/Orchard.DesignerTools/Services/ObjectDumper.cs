using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DesignerTools.Services {
    
    public class ObjectDumper {
        private const int MaxStringLength = 60;

        private readonly Stack<object> _parents = new Stack<object>();
        private readonly Stack<XElement> _currents = new Stack<XElement>();
        private readonly int _levels;

        private readonly XDocument _xdoc;
        private XElement _current;

        // object/key/dump

        public ObjectDumper(int levels)
        {
            _levels = levels;
            _xdoc = new XDocument();
            _xdoc.Add(_current = new XElement("ul"));
        }

        public XElement Dump(object o, string name) {
            if(_parents.Count >= _levels) {
                return _current;
            }

            _parents.Push(o);
            
            // starts a new container
            EnterNode("li");

            try {
                if (o == null) {
                    DumpValue(null, name);
                }
                else if (o.GetType().IsValueType || o is string) {
                    DumpValue(o, name);
                }
                else {
                    if (_parents.Count >= _levels) {
                        return _current;
                    }

                    DumpObject(o, name);
                }
            }
            finally {
                _parents.Pop(); 
                RestoreCurrentNode();
            }
        
            return _current;
        }

        private void DumpValue(object o, string name) {
            string formatted = FormatValue(o);
            _current.Add(
                new XElement("h1", new XText(name)),
                new XElement("span", formatted)
            );
        }

        private void DumpObject(object o, string name) {
            _current.Add(
                new XElement("h1", new XText(name)),
                new XElement("span", FormatType(o))
            ); 
            
            EnterNode("ul");

            try {
                if (o is IDictionary) {
                    DumpDictionary((IDictionary) o);
                }
                else if (o is IShape) {
                    DumpShape((IShape) o);

                    // a shape can also be IEnumerable
                    if (o is Shape) {
                        var items = o.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "Items");

                        var classes = o.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "Classes");

                        var attributes = o.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "Attributes");

                        if (classes != null) {
                            DumpMember(o, classes);
                        }

                        if (attributes != null) {
                            DumpMember(o, attributes);
                        }

                        if (items != null) {
                            DumpMember(o, items);
                        }
                        

                        // DumpEnumerable((IEnumerable) o);
                    }
                }
                else if (o is IEnumerable) {
                    DumpEnumerable((IEnumerable) o);
                }
                else {
                    DumpMembers(o);
                }
            }
            finally {
                RestoreCurrentNode();
            }
        }

        private void DumpMembers(object o) {
            var members = o.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                .Union(o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                .Where(m => !m.Name.StartsWith("_")) // remove members with a name starting with '_' (usually proxied objects)
                .ToList();

            if(!members.Any()) {
                return;
            }

            foreach (var member in members) {
                if ((o is ContentItem && (member.Name == "ContentManager"
                        || member.Name == "Parts"
                        || member.Name == "Record"
                        || member.Name == "VersionRecord"
                        || member.Name == "TypeDefinition" 
                        || member.Name == "TypePartDefinition" 
                        || member.Name == "PartDefinition"))
                    || o is Delegate
                    || o is Type
                    ) {
                    continue;
                }

                if ((o is ContentPart && (member.Name == "ContentItem"))) {
                    continue;
                }

                SafeCall(() => DumpMember(o, member));
            }

            // process ContentItem.Parts specifically
            foreach (var member in members) {
                if (o is ContentItem && member.Name == "Parts") {
                    foreach (var part in ((ContentItem) o).Parts) {
                        // ignore contentparts like ContentPart<ContentItemVersionRecord>
                        if(part.GetType().IsGenericType) {
                            continue;
                        }

                        SafeCall(() => Dump(part, part.PartDefinition.Name));
                    }
                }
            }

            foreach (var member in members) {
                // process ContentPart.Fields specifically
                if (o is ContentPart && member.Name == "Fields") {
                    foreach (var field in ((ContentPart) o).Fields) {
                        SafeCall(() => Dump(field, field.Name));
                    }
                }
            }
        }

        private void DumpEnumerable(IEnumerable enumerable) {
            if (!enumerable.GetEnumerator().MoveNext()) {
                return;
            }

            int i = 0;
            foreach (var child in enumerable) {
                Dump(child, string.Format("[{0}]", i++));
            }
        }

        private void DumpDictionary(IDictionary dictionary) {
            if (dictionary.Keys.Count == 0) {
                return;
            }

            foreach (var key in dictionary.Keys) {
                Dump(dictionary[key], string.Format("[\"{0}\"]", key));
            }
        }

        private void DumpShape(IShape shape) {
            var value = shape as Shape;

            if (value == null) {
                return;
            }

            foreach (DictionaryEntry entry in value.Properties) {
                // ignore private members (added dynamically by the shape wrapper)
                if (entry.Key.ToString().StartsWith("_")) {
                    continue;
                }

                Dump(entry.Value, entry.Key.ToString());
            }
        }

        private void DumpMember(object o, MemberInfo member) {
            if (member is MethodBase || member is EventInfo)
                return;

            if (member is FieldInfo) {
                var field = (FieldInfo)member;
                Dump(field.GetValue(o), member.Name);
            }
            else if (member is PropertyInfo) {
                var prop = (PropertyInfo)member;

                if (prop.GetIndexParameters().Length == 0 && prop.CanRead) {
                    Dump(prop.GetValue(o, null), member.Name);
                }
            }
        }

        private static string FormatValue(object o) {
            if (o == null)
                return "null";

            var formatted = o.ToString();

            if (o is string) {
                // remove central part if tool long
                if(formatted.Length > MaxStringLength) {
                    formatted = formatted.Substring(0, MaxStringLength/2) + "..." + formatted.Substring(formatted.Length - MaxStringLength/2);
                }

                formatted = "\"" + formatted + "\"";
            }

            return formatted;
        }

        private static string FormatType(object item) {
            var shape = item as IShape;
            if (shape != null) {
                return shape.Metadata.Type + " Shape";
            }

            return FormatType(item.GetType());
        }

        private static string FormatType(Type type) {
            if (type.IsGenericType) {
                var genericArguments = String.Join(", ", type.GetGenericArguments().Select(FormatType).ToArray());
                return String.Format("{0}<{1}>", type.Name.Substring(0, type.Name.IndexOf('`')), genericArguments);
            }

            return type.Name;
        }

        private static void SafeCall(Action action) {
            try {
                action();
            }
            catch {
                // ignore exceptions is safe call
            }
        }

        private void SaveCurrentNode() {
            _currents.Push(_current);
        }

        private void RestoreCurrentNode() {
            if (!_current.DescendantNodes().Any()) {
                _current.Remove();
            }
            
            _current = _currents.Pop();
        }

        private void EnterNode(string tag) {
            SaveCurrentNode();
            _current.Add(_current = new XElement(tag));
        }

        public static void ConvertToJSon(XElement x, StringBuilder sb) {
            if (x == null) {
                return;
            }

            switch (x.Name.ToString()) {
                case "ul":
                    var first = true;
                    foreach (var li in x.Elements()) {
                        if (!first) sb.Append(",");
                        ConvertToJSon(li, sb);
                        first = false;
                    }
                    break;
                case "li":
                    var name = x.Element("h1").Value;
                    var value = x.Element("span").Value;

                    sb.AppendFormat("\"name\": \"{0}\", ", FormatJsonValue(name));
                    sb.AppendFormat("\"value\": \"{0}\"", FormatJsonValue(value));

                    var ul = x.Element("ul");
                    if (ul != null && ul.Descendants().Any()) {
                        sb.Append(", \"children\": [");
                        first = true;
                        foreach (var li in ul.Elements()) {
                            sb.Append(first ? "{ " : ", {");
                            ConvertToJSon(li, sb);
                            sb.Append(" }");
                            first = false;
                        }
                        sb.Append("]");
                    }

                    break;
            }
        }

        public static string FormatJsonValue(string value) {
            if (String.IsNullOrEmpty(value)) {
                return String.Empty;
            }

            // replace " by \" in json strings
            return HttpUtility.HtmlEncode(value).Replace(@"\", @"\\").Replace("\"", @"\""").Replace("\r\n", @"\n").Replace("\r", @"\n").Replace("\n", @"\n");
        }
    }
}