using System;
using System.Web.Mvc;
using ClaySharp;

namespace Orchard.DisplayManagement.Implementation {
    public class DisplayHelperFactory : IDisplayHelperFactory {
        static private readonly DisplayHelperBehavior[] _behaviors = new[] { new DisplayHelperBehavior() };
        private readonly IDisplayManager _displayManager;
        private readonly IShapeFactory _shapeFactory;

        public DisplayHelperFactory(IDisplayManager displayManager, IShapeFactory shapeFactory) {
            _displayManager = displayManager;
            _shapeFactory = shapeFactory;
        }

        public dynamic CreateHelper(ViewContext viewContext, IViewDataContainer viewDataContainer) {
            return ClayActivator.CreateInstance<DisplayHelper>(
                _behaviors,
                _displayManager,
                _shapeFactory,
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