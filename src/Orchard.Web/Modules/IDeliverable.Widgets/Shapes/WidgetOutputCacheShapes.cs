using System.IO;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Shapes
{
    [OrchardFeature("IDeliverable.Widgets.OutputCache")]
    public class WidgetOutputCacheShapes : IDependency
    {
        [Shape]
        public void RawOutput(dynamic Shape, TextWriter Output, string Content)
        {
            if (Content == null)
                return;

            Output.Write(Content);
        }
    }
}