using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;

namespace Orchard.DisplayManagement.Secondary {
    public class DefaultDisplayManager : IDisplayManager {
        private readonly IShapeTableFactory _shapeTableFactory;

        public DefaultDisplayManager(IShapeTableFactory shapeTableFactory) {
            _shapeTableFactory = shapeTableFactory;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public object Execute(Shape shape, ViewContext viewContext, IViewDataContainer viewDataContainer) {
            ShapeAttributes shapeAttributes = ((dynamic)shape).Attributes;
            var shapeTable = _shapeTableFactory.CreateShapeTable();
            ShapeTable.Entry entry;
            if (shapeTable.Entries.TryGetValue(shapeAttributes.Type, out entry)) {
                return Process(entry, shape, viewContext, viewDataContainer);
            }
            throw new OrchardException(T("Shape name not found"));
        }

        private object Process(ShapeTable.Entry entry, Shape shape, ViewContext viewContext, IViewDataContainer viewDataContainer) {
            var displayContext = new DisplayContext { ViewContext = viewContext, Shape = shape };
            return entry.Target(displayContext);
        }
    }
}