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
            get { return "Forms"; }
        }

        public virtual string Name {
            get { return this.Retrieve<string>("InputName"); }
        }

        public string Value {
            get { return this.Retrieve(x => x.Value); }
        }

        /// <summary>
        /// The runtime value for this element. This is either the default value, or the submitted value.
        /// </summary>
        public string RuntimeValue {
            get { return _runtimeValue.Value; }
            set { _runtimeValue = new Lazy<string>(() => value); }
        }

        /// <summary>
        /// The value posted back for this element.
        /// </summary>
        public string PostedValue { get; set; }

        public string FormBindingContentType {
            get { return this.Retrieve(x => x.FormBindingContentType); }
            set { this.Store(x => x.FormBindingContentType, value); }
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