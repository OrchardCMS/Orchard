using System;
using System.Web.Mvc;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public class DisplayHelperFactory : IDisplayHelperFactory {
        static private readonly DisplayHelperBehavior[] _behaviors = new[] { new DisplayHelperBehavior() };
        private readonly IDisplayManager _displayManager;
        private readonly IShapeBuilder _shapeBuilder;

        public DisplayHelperFactory(IDisplayManager displayManager, IShapeBuilder shapeBuilder) {
            _displayManager = displayManager;
            _shapeBuilder = shapeBuilder;
        }

        public DisplayHelper CreateDisplayHelper(ViewContext viewContext, IViewDataContainer viewDataContainer) {
            return (DisplayHelper)ClayActivator.CreateInstance<DisplayHelper>(
                _behaviors,
                _displayManager,
                _shapeBuilder,
                viewContext,
                viewDataContainer);
        }

        class DisplayHelperBehavior : ClayBehavior {
            public override object InvokeMember(Func<object> proceed, object target, string name, INamedEnumerable<object> args) {
                return ((DisplayHelper)target).Invoke(name, args);
            }
        }
    }
}