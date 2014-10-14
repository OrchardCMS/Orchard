using System;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public abstract class FormElement : Element {
        private Lazy<string> _runtimeValue;

        protected FormElement() {
            _runtimeValue = new Lazy<string>(() => Value);
        }

        public override string Category {
            get { return "Form"; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public virtual string Name {
            get { return State.Get("InputName"); }
        }

        public string Value {
            get { return State.Get("Value"); }
        }

        public string RuntimeValue {
            get { return _runtimeValue.Value; }
            set { _runtimeValue = new Lazy<string>(() => value); }
        }

        public string FormBindingContentType {
            get { return State.Get("FormBindingContentType"); }
            set { State["FormBindingContentType"] = value; }
        }

        public Form Form {
            get {
                var parent = Container;

                while (parent != null) {
                    var form = parent as Form;

                    if (form != null)
                        return form;

                    parent = parent.Container;
                }

                return null;
            }
        }
    }
}