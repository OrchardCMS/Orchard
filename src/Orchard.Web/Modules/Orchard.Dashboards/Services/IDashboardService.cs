using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement;

namespace Orchard.Dashboards.Services {
    public interface IDashboardService : IDependency {
        dynamic GetDashboardShape();
    }

    public class DashboardService : IDashboardService {
        private readonly Lazy<IEnumerable<IDashboardSelector>> _selectors;
        private readonly IShapeFactory _shapeFactory;

        public DashboardService(Lazy<IEnumerable<IDashboardSelector>> selectors, IShapeFactory shapeFactory) {
            _selectors = selectors;
            _shapeFactory = shapeFactory;
        }

        public dynamic GetDashboardShape() {
            var selectorQuery = 
                from selector in _selectors.Value
                let descriptor = selector.GetDashboardDescriptor()
                orderby descriptor.Priority descending
                select descriptor;

            var result = selectorQuery.First();
            var factory = result.DashboardFactory ?? (shapeFactory => shapeFactory.StaticDashboard());
            var shape = factory(_shapeFactory);

            return shape;
        }
    }
}