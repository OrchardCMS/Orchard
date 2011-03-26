using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ClaySharp;
using ClaySharp.Behaviors;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;

namespace Orchard.DesignerTools.Services {

    public class ObjectDumper {
        private const int MaxStringLength = 60;

        private readonly Stack<object> _parents = new Stack<object>();
        private readonly int _levels;

        private readonly XDocument _xdoc;
        private XElement _node;
        
        public ObjectDumper(int levels) {
            _levels = levels;
            _xdoc = new XDocument();
            _xdoc.Add(_node = new XElement("ul"));
        }

        public XElement Dump(object o, string name) {
            // prevent cyclic references
            if (_parents.Contains(o)) {
                return _node;
            }

            if(_parents.Count >= _levels) {
                return _node;
            }

            _parents.Push(o);
            try {
                // starts a new container
                _node.Add(_node = new XElement("li"));

                if (o == null) {
                    DumpValue(null, name);
                }
                else if (o.GetType().IsValueType || o is string) {
                    DumpValue(o, name);
                }
                else {
                    DumpObject(o, name);
                }
            }
            finally { 
                _parents.Pop(); 
            }

            if(_node.DescendantNodes().Count() == 0) {
                _node.Remove();
            }
            _node = _node.Parent;

            return _node;
        }

        private void DumpValue(object o, string name) {
            string formatted = FormatValue(o);
            _node.Add(
                new XElement("div", new XAttribute("class", "name"), name),
                new XElement("div", new XAttribute("class", "value"), formatted)
            );
        }

        private void DumpObject(object o, string name) {
            _node.Add(
                new XElement("div", new XAttribute("class", "name"), name),
                new XElement("div", new XAttribute("class", "type"), FormatType(o))
            );

            if (_parents.Count >= _levels) {
                return;
            }

            if (o is IDictionary) {
                DumpDictionary((IDictionary)o);
            }
            else if (o is IShape) {
                DumpShape((IShape)o);
                
                // a shape can also be IEnumerable
                if (o is IEnumerable) {
                    DumpEnumerable((IEnumerable) o);
                }
            }
            else if (o is IEnumerable)
            {
                DumpEnumerable((IEnumerable)o);
            }
            else {
                DumpMembers(o);
            }
        }

        private void DumpMembers(object o) {
            var members = o.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                .Union(o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                .Where(m => !m.Name.StartsWith("_")) // remove members with a name starting with '_' (usually proxied objects)
                .ToList();

            if(members.Count() == 0) {
                return;
            }

            _node.Add(_node = new XElement("ul"));
            foreach (var member in members) {
                if (o is ContentItem && member.Name == "ContentManager") {
                    // ignore Content Manager explicitly
                    continue;
                }

                try {
                    DumpMember(o, member);
                }
                catch {
                    // ignore members which can't be rendered
                }

                // process ContentItem.Parts specifically
                if (o is ContentItem && member.Name == "Parts") {
                    foreach (var part in ((ContentItem)o).Parts) {
                        Dump(part, part.PartDefinition.Name);
                    }
                }

                // process ContentPart.Fields specifically
                if (o is ContentPart && member.Name == "Fields") {
                    foreach (var field in ((ContentPart)o).Fields) {
                        Dump(field, field.Name);
                    }
                }
            }

            _node = _node.Parent;
        }

        private void DumpEnumerable(IEnumerable enumerable) {
            if(!enumerable.GetEnumerator().MoveNext()) {
                return;
            }

            _node.Add(_node = new XElement("ul"));
            int i = 0;
            foreach (var child in enumerable) {
                Dump(child, string.Format("[{0}]", i++));
            }
            
            _node = _node.Parent;
        }

        private void DumpDictionary(IDictionary dictionary) {
            if (dictionary.Keys.Count == 0) {
                return;
            }
            _node.Add(_node = new XElement("ul"));
            foreach (var key in dictionary.Keys) {
                Dump(dictionary[key], string.Format("[\"{0}\"]", key));
            }
            _node = _node.Parent;
        }

        private void DumpShape(IShape shape) {

            var b = ((IClayBehaviorProvider)(dynamic)shape).Behavior as ClayBehaviorCollection;

            if (b == null)
                return;

            // seek the PropBehavior if exists
            var propBehavior = b.OfType<PropBehavior>().FirstOrDefault();

            if (propBehavior == null)
                return;

            // retrieve the internal dictionary for properties
            var props = propBehavior.GetType().GetField("_props", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(propBehavior) as Dictionary<object, object>;

            if (props == null)
                return;

            if (props.Keys.Count == 0) {
                return;
            }

            _node.Add(_node = new XElement("ul"));
            foreach (var key in props.Keys) {
                // ignore private members (added dynmically by the shape wrapper)
                if(key.ToString().StartsWith("_")) {
                    continue;
                }
                Dump(props[key], key.ToString());
            }
            _node = _node.Parent;
        }

        private void DumpMember(object o, MemberInfo member) {
            if (member is MethodInfo || member is ConstructorInfo || member is EventInfo)
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
            if(item is IShape) {
                return ((IShape)item).Metadata.Type + " Shape";
            }

            return FormatType(item.GetType());
        }

        private static string FormatType(Type type) {
            if (type.IsGenericType) {
                var genericArguments = String.Join(", ", type.GetGenericArguments().Select(t => FormatType((Type)t)).ToArray());
                return String.Format("{0}<{1}>", type.Name.Substring(0, type.Name.IndexOf('`')), genericArguments);
            }

            return type.Name;
        }
    }
}