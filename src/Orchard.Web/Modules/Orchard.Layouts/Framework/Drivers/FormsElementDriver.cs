using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Framework.Drivers {
    public abstract class FormsElementDriver<TElement> : ElementDriver<TElement>, IFormProvider where TElement : Element {
        private readonly IFormManager _formManager;
        private readonly ICultureAccessor _cultureAccessor;

        protected FormsElementDriver(IFormsBasedElementServices formsServices) {
            _formManager = formsServices.FormManager;
            _cultureAccessor = formsServices.CultureAccessor;
        }

        protected dynamic BuildForm(ElementEditorContext context, string formName, string position = null) {
            // TODO: Fix Forms API so that it works with prefixes. Right now only binding implements prefix, but building a form ignores the specified prefix.

            // If not a post-back, we need to bind the form with the element's data values. Otherwise, bind the form with the posted values.
            var valueProvider = context.Updater == null
                ? context.Element.Data.ToValueProvider(_cultureAccessor.CurrentCulture)
                : context.ValueProvider;

            var form = _formManager.Bind(_formManager.Build(formName), valueProvider);

            if (context.Updater != null) {
                // Update the element's data dictionary with the posted values.
                Action<object> process = s => UpdateElementProperty(s, context);
                FormNodesProcessor.ProcessForm(form, process);
            }

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

            // If not a post-back, we need to bind the form with the element's data values. Otherwise, bind the form with the posted values.
            var valueProvider = context.Updater == null
                ? context.Element.Data.ToValueProvider(_cultureAccessor.CurrentCulture)
                : context.ValueProvider;

            var forms = FormNames.Reverse().Select(x => {
                var shape = _formManager.Bind(_formManager.Build(x), valueProvider);

                if (context.Updater != null) {
                    // Update the element's data dictionary with the posted values.
                    Action<object> process = s => UpdateElementProperty(s, context);
                    FormNodesProcessor.ProcessForm(shape, process);
                }

                return shape;
            }).ToArray();
            var formShape = context.ShapeFactory.ElementEditor__Forms(Forms: forms);
            return formShape;
        }

        private void UpdateElementProperty(dynamic formElementShape, ElementEditorContext context) {
            var name = (string)formElementShape.Name;
            if (name != null) {
                var value = context.ValueProvider.GetValue(context.Prefix + name);
                if (value != null) {
                    context.Element.Data[name] = value.AttemptedValue;
                }
                else if (formElementShape.Metadata.Type == "Checkbox") {
                    var shapeValue = formElementShape.Value as string;
                    if (shapeValue != null && shapeValue.ToLower() == "true") {
                        context.Element.Data[name] = "false";
                    }
                }
            }
        }

        public void Describe(DescribeContext context) {
            DescribeForm(context);
        }

        protected virtual void DescribeForm(DescribeContext context) { }
    }
}
