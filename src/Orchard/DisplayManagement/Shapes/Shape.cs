using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using ClaySharp;
using ClaySharp.Implementation;

namespace Orchard.DisplayManagement.Shapes {
    [DebuggerTypeProxy(typeof(ShapeDebugView))]
    public class Shape : IShape, IEnumerable {
        private const string DefaultPosition = "5";

        private readonly IList<object> _items = new List<object>();
        private readonly IList<string> _classes = new List<string>();
        private readonly IDictionary<string, string> _attributes = new Dictionary<string, string>();

        public virtual ShapeMetadata Metadata { get; set; }

        public virtual string Id { get; set; }
        public virtual IList<string> Classes { get { return _classes; } }
        public virtual IDictionary<string, string> Attributes { get { return _attributes; } }
        public virtual IEnumerable<dynamic> Items { get { return _items; } }

        public virtual Shape Add(object item, string position = null) {
            // pszmyd: Ignoring null shapes 
            if (item == null) {
                return this;
            }

            try {
                // todo: (sebros) this is a temporary implementation to prevent common known scenarios throwing exceptions. The final solution would need to filter based on the fact that it is a Shape instance
                if ( item is MvcHtmlString ||
                    item is String ) {
                    // need to implement positioned wrapper for non-shape objects
                }
                else if (item is IShape) {
                    ((dynamic) item).Metadata.Position = position;
                }
            }
            catch {
                // need to implement positioned wrapper for non-shape objects
            }

            _items.Add(item); // not messing with position at the moment
            return this;
        }

        public virtual Shape AddRange(IEnumerable<object> items, string position = DefaultPosition) {
            foreach (var item in items)
                Add(item, position);
            return this;
        }

        public virtual IEnumerator GetEnumerator() {
            return _items.GetEnumerator();
        }

        public class ShapeBehavior : ClayBehavior {
            public override object SetIndex(Func<object> proceed, dynamic self, IEnumerable<object> keys, object value) {
                if (keys.Count() == 1) {
                    var name = keys.Single().ToString();
                    if (name.Equals("Id")) {
                        // need to mutate the actual type
                        var s = self as Shape;
                        if (s != null) {
                            s.Id = System.Convert.ToString(value);
                        }
                        return value;
                    }
                    if (name.Equals("Classes")) {
                        var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                        MergeClasses(args, self.Classes);
                        return value;
                    }
                    if (name.Equals("Attributes")) {
                        var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                        MergeAttributes(args, self.Attributes);
                        return value;
                    }
                    if (name.Equals("Items")) {
                        var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                        MergeItems(args, self);
                        return value;
                    }
                }
                return proceed();
            }

            public override object InvokeMember(Func<object> proceed, dynamic self, string name, INamedEnumerable<object> args) {
                if (name.Equals("Id")) {
                    // need to mutate the actual type
                    var s = self as Shape;
                    if (s != null) {
                        s.Id = System.Convert.ToString(args.FirstOrDefault());
                    }
                    return self;
                }
                if (name.Equals("Classes") && !args.Named.Any()) {
                    MergeClasses(args, self.Classes);
                    return self;
                }
                if (name.Equals("Attributes") && args.Positional.Count() <= 1) {
                    MergeAttributes(args, self.Attributes);
                    return self;
                }
                if (name.Equals("Items")) {
                    MergeItems(args, self);
                    return self;
                }
                return proceed();
            }

            private static void MergeAttributes(INamedEnumerable<object> args, IDictionary<string, string> attributes) {
                var arg = args.Positional.SingleOrDefault();
                if (arg != null) {
                    if (arg is IDictionary) {
                        var dictionary = arg as IDictionary;
                        foreach (var key in dictionary.Keys) {
                            attributes[System.Convert.ToString(key)] = System.Convert.ToString(dictionary[key]);
                        }
                    }
                    else {
                        foreach (var prop in arg.GetType().GetProperties()) {
                            attributes[TranslateIdentifier(prop.Name)] = System.Convert.ToString(prop.GetValue(arg, null));
                        }
                    }
                }
                foreach (var named in args.Named) {
                    attributes[named.Key] = System.Convert.ToString(named.Value);
                }
            }

            private static string TranslateIdentifier(string name) {
                // Allows for certain characters in an identifier to represent different
                // characters in an HTML attribute (mimics MVC behavior):
                // data_foo ==> data-foo
                // @keyword ==> keyword
                return name.Replace("_", "-").Replace("@", "");
            }

            private static void MergeClasses(INamedEnumerable<object> args, IList<string> classes) {
                foreach (var arg in args) {
                    // look for string first, because the "string" type is also an IEnumerable of char
                    if (arg is string) {
                        classes.Add(arg as string);
                    }
                    else if (arg is IEnumerable) {
                        foreach (var item in arg as IEnumerable) {
                            classes.Add(System.Convert.ToString(item));
                        }
                    }
                    else {
                        classes.Add(System.Convert.ToString(arg));
                    }
                }
            }

            private static void MergeItems(INamedEnumerable<object> args, dynamic shape) {
                foreach (var arg in args) {
                    // look for string first, because the "string" type is also an IEnumerable of char
                    if (arg is string) {
                        shape.Add(arg as string);
                    }
                    else if (arg is IEnumerable) {
                        foreach (var item in arg as IEnumerable) {
                            shape.Add(item);
                        }
                    }
                    else {
                        shape.Add(System.Convert.ToString(arg));
                    }
                }
            }
        }
    }
}
