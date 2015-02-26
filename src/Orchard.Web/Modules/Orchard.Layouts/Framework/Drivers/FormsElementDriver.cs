using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Drivers {
    public abstract class FormsElementDriver<TElement> : ElementDriver<TElement>, IFormProvider where TElement : Element {
        private readonly IFormManager _formManager;

        protected FormsElementDriver(IFormManager formManager) {
            _formManager = formManager;
        }

        protected dynamic BuildForm(ElementEditorContext context, string formName, string position = null) {
            // TODO: Fix Forms API so that it works with prefixes. Right now only binding implements prefix, but building a form ignores the specified prefix.
            var form = _formManager.Bind(_formManager.Build(formName), context.ValueProvider);

            if (!String.IsNullOrWhiteSpace(position)) {
                form.Metadata.Position = position;
            }

            return form;
        }

        protected virtual IEnumerable<string> FormNames {
            get { yield break; }
        }

        protected override EditorResult OnBuildEditor(TElement element, ElementEditorContext context) {
            var formShape = BuildForms(context);
            return Editor(context, formShape);
        }

        protected dynamic BuildForms(ElementEditorContext context) {
            // TODO: Fix Forms API so that it works with prefixes. Right now only binding implements prefix, but building a form ignores the specified prefix.
            var forms = FormNames.Reverse().Select(x => _formManager.Bind(_formManager.Build(x), context.ValueProvider)).ToArray();
            var formShape = context.ShapeFactory.ElementEditor__Forms(Forms: forms);
            return formShape;
        }

        public void Describe(DescribeContext context) {
            DescribeForm(context);
        }

        protected virtual void DescribeForm(DescribeContext context) { }
    }
}