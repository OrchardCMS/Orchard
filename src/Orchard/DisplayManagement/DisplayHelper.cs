using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

            if (parameters.Positional.Count() == 1) {
                return ShapeExecute(parameters.Positional.Single());
            }

            if (parameters.Positional.Any()) {
                return new Combined(ShapeExecute(parameters.Positional));
            }

            // zero args - no display to execute
            return null;
        }

        public class Combined : IHtmlString {
            private readonly IEnumerable<object> _fragments;

            public Combined(IEnumerable<object> fragments) {
                _fragments = fragments;
            }

            public string ToHtmlString() {
                return _fragments.Aggregate("", (a, b) => a + b);
            }
            public override string ToString() {
                return ToHtmlString();
            }
        }

        private object ShapeTypeExecute(string name, INamedEnumerable<object> parameters) {
            var shape = _shapeBuilder.Build(name, parameters);
            return ShapeExecute(shape);
        }

        public object ShapeExecute(object shape) {
            var context = new DisplayContext { Display = this, Value = shape, ViewContext = ViewContext };
            return _displayManager.Execute(context);
        }

        public IEnumerable<object> ShapeExecute(IEnumerable<object> shapes) {
            return shapes.Select(ShapeExecute).ToArray();
        }
    }
}
