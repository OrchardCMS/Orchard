using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Logging;

namespace Orchard.Forms.Services {
    public class DefaultFormManager : IFormManager {
        private readonly IEnumerable<IFormProvider> _formProviders;
        private readonly IEnumerable<IFormEventHandler> _formEventHandlers;
        private readonly IShapeFactory _shapeFactory;

        public ILogger Logger { get; set; }

        public DefaultFormManager(IEnumerable<IFormProvider> formProviders, IEnumerable<IFormEventHandler> formEventHandlers, IShapeFactory shapeFactory) {
            _formProviders = formProviders;
            _formEventHandlers = formEventHandlers;
            _shapeFactory = shapeFactory;
        }

        public dynamic Build(string formName, string prefix = "") {
            var context = new DescribeContext();
            foreach (var provider in _formProviders) {
                provider.Describe(context);
            }

            var descriptor = context.Describe().FirstOrDefault(x => x.Name == formName);

            if (descriptor == null) {
                return null;
            }

            var shape = descriptor.Shape(_shapeFactory);
            var buildingContext = new BuildingContext { Shape = shape };

            _formEventHandlers.Invoke(dispatch => dispatch.Building(buildingContext), Logger);
            // alter the shapes tree (validation, ajax, ...));
            _formEventHandlers.Invoke(dispatch => dispatch.Built(buildingContext), Logger);

            return shape;
        }

        public dynamic Bind(dynamic formShape, IValueProvider valueProvider, string prefix = "") {
            Action<object> process = shape => BindValue(shape, valueProvider, prefix);
            FormNodesProcessor.ProcessForm(formShape, process);

            return formShape;
        }

        public string Export(string formName, string state, ExportContentContext exportContext) {
            var describeContext = new DescribeContext();
            foreach (var provider in _formProviders) {
                provider.Describe(describeContext);
            }

            var descriptor = describeContext.Describe().FirstOrDefault(x => x.Name == formName);

            if (descriptor == null || descriptor.Export == null) {
                return state;
            }

            var dynamicState = FormParametersHelper.ToDynamic(state);
            descriptor.Export(dynamicState, exportContext);

            return FormParametersHelper.ToString(dynamicState);
        }

        public string Import(string formName, string state, ImportContentContext importContext) {
            var describeContext = new DescribeContext();
            foreach (var provider in _formProviders) {
                provider.Describe(describeContext);
            }

            var descriptor = describeContext.Describe().FirstOrDefault(x => x.Name == formName);

            if (descriptor == null || descriptor.Import == null) {
                return state;
            }

            var dynamicState = FormParametersHelper.ToDynamic(state);
            descriptor.Import(dynamicState, importContext);

            return FormParametersHelper.ToString(dynamicState);
        }

        private static void BindValue(dynamic shape, IValueProvider valueProvider, string prefix) {
            // if the shape has a Name property, look for a value in
            // the ValueProvider
            var name = shape.Name;
            if (name != null) {
                ValueProviderResult value = valueProvider.GetValue(prefix + name);
                if (value != null) {
                    if (shape.Metadata.Type == "Checkbox" || shape.Metadata.Type == "Radio") {
                        shape.Checked = Convert.ToString(shape.Value) == Convert.ToString(value.AttemptedValue);
                    }
                    else {
                        shape.Value = value.AttemptedValue;
                    }
                    
                }
            }
        }

        public void Validate(ValidatingContext context) {
            _formEventHandlers.Invoke(dispatch => dispatch.Validating(context), Logger);
            _formEventHandlers.Invoke(dispatch => dispatch.Validated(context), Logger);
        }
    }
}