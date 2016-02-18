using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.Drivers
{
    internal static class DriverResultExtensions {
        public static IEnumerable<ContentShapeResult> GetShapeResults(this DriverResult driverResult) {
            if (driverResult is CombinedResult) {
                return ((CombinedResult)driverResult).GetResults().Select(result => result as ContentShapeResult);
            }

            return new[] { driverResult as ContentShapeResult };
        }
    }
}