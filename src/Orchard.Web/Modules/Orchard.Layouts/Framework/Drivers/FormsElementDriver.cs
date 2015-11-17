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
            UpdateElementData(context, new[] { formName });
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

        /// <summary>
        /// If we are currently updating the element, then based on the involved forms,
        /// update the Element's data dictionary with submitted data.
        /// </summary>
        protected void UpdateElementData(ElementEditorContext context, IEnumerable<string> formNames, string prefix = "") {

            if (context.Updater != null) {

                // Build forms and look for entires in the Element Data that have the same name.
                foreach (var formName in formNames) {
                    var form = _formManager.Build(formName);
                    FormNodesProcessor.ProcessForm(form, (Action<object>) (shape => {
                        var name = ((dynamic) shape).Name as string;
                        if (name != null) {
                            var value = context.ValueProvider.GetValue(prefix + name);
                            if (value != null) {
                                context.Element.Data[name] = value.AttemptedValue;
                            }
                        }
                    }));
                }
            }
        }

        protected dynamic BuildForms(ElementEditorContext context) {
            // TODO: Fix Forms API so that it works with prefixes. Right now only binding implements prefix, but building a form ignores the specified prefix.
            UpdateElementData(context, FormNames);
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