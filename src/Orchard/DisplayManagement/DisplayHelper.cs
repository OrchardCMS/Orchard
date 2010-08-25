using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {


    public class DisplayHelper {
        private readonly IDisplayManager _displayManager;
        private readonly IShapeBuilder _shapeBuilder;

        public DisplayHelper(
            IDisplayManager displayManager,
            IShapeBuilder shapeBuilder,
            ViewContext viewContext,
            IViewDataContainer viewDataContainer) {
            _displayManager = displayManager;
            _shapeBuilder = shapeBuilder;
            ViewContext = viewContext;
            ViewDataContainer = viewDataContainer;
        }

        public ViewContext ViewContext { get; set; }
        public IViewDataContainer ViewDataContainer { get; set; }

        public object Invoke(string name, INamedEnumerable<object> parameters) {
            if (!string.IsNullOrEmpty(name)) {
                return ShapeTypeExecute(name, parameters);
            }

            if (parameters.Positional.Count() == 1 && parameters.Positional.All(arg => arg is Shape)) {
                var shape = (Shape)parameters.Positional.Single();
                return ShapeExecute(shape);
            }

            throw new NotImplementedException("Need to handle multiple shapes, as well as other object types");
        }

        private object ShapeTypeExecute(string name, INamedEnumerable<object> parameters) {
            var shape = _shapeBuilder.Build(name, parameters);
            return ShapeExecute(shape);
        }

        public object ShapeExecute(Shape shape) {
            return _displayManager.Execute(shape, ViewContext, ViewDataContainer);
        }
    }
}
