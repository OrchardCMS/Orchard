using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ClaySharp;
using ClaySharp.Behaviors;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;

namespace Orchard.DesignerTools.Services {

    public class ObjectDumper {
        private const int MaxStringLength = 60;

        private readonly Stack<object> _parents = new Stack<object>();
        private readonly Stack<XElement> _currents = new Stack<XElement>();
        private readonly int _levels;
        private readonly Dictionary<int, XElement> _local;
        private readonly Dictionary<int, XElement> _global;

        private readonly XDocument _xdoc;
        private XElement _current;

        // object/key/dump

        public ObjectDumper(int levels, Dictionary<int, XElement> local, Dictionary<int, XElement> global) {
            _levels = levels;
            _local = local;
            _global = global;
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
                    int hashCode = RuntimeHelpers.GetHashCode(o);
                    // if the object has already been processed, return a named ref to it
                    if (_global.ContainsKey(hashCode)) {
                        _current.Add(
                            new XElement("h1", new XText(name)),
                            new XElement("span", FormatType(o)),
                            new XElement("a", new XAttribute("href", hashCode.ToString()))
                        );
                    }
                    else {
                        _global.Add(hashCode, _current);
                        _local.Add(hashCode, _current);
                        DumpObject(o, name);
                    }
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

            EnterNode("ul");

            try{
                foreach (var member in members) {
                    if (o is ContentItem && member.Name == "ContentManager"
                        || o is Delegate) {
                        continue;
                    }
                    SafeCall(() => DumpMember(o, member));
                }

                // process ContentItem.Parts specifically
                foreach (var member in members) {
                    if (o is ContentItem && member.Name == "Parts") {
                        foreach (var part in ((ContentItem) o).Parts) {
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
            finally {
                RestoreCurrentNode();
            }
        }

        private void DumpEnumerable(IEnumerable enumerable) {
            if(!enumerable.GetEnumerator().MoveNext()) {
                return;
            }

            EnterNode("ul");

            try {
                int i = 0;
                foreach (var child in enumerable) {
                    Dump(child, string.Format("[{0}]", i++));
                }
            }
            finally {
               RestoreCurrentNode();
            }
        }

        private void DumpDictionary(IDictionary dictionary) {
            if (dictionary.Keys.Count == 0) {
                return;
            }

            EnterNode("ul");

            try {
                foreach (var key in dictionary.Keys) {
                    Dump(dictionary[key], string.Format("[\"{0}\"]", key));
                }
            }
            finally {
                RestoreCurrentNode();
            }
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

            EnterNode("ul");

            try {
                foreach (var key in props.Keys) {
                    // ignore private members (added dynmically by the shape wrapper)
                    if (key.ToString().StartsWith("_")) {
                        continue;
                    }
                    Dump(props[key], key.ToString());
                }
            }
            finally {
                RestoreCurrentNode();
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
            if (_current.DescendantNodes().Count() == 0) {
                _current.Remove();
            }
            
            _current = _currents.Pop();
        }

        private void EnterNode(string tag) {
            SaveCurrentNode();
            _current.Add(_current = new XElement(tag));
        }
    }
}