using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class ContentShapeResult : AsyncContentShapeResult {
        public ContentShapeResult(string shapeType, string prefix, Func<BuildShapeContext, dynamic> shapeBuilder)
            : base(shapeType, prefix, ctx => Task.FromResult<dynamic>(shapeBuilder(ctx))) {
        }

        public new ContentShapeResult Location(string zone) {
            base.Location(zone);
            return this;
        }

        public new ContentShapeResult Differentiator(string differentiator) {
            base.Differentiator(differentiator);
            return this;
        }

        public new ContentShapeResult OnGroup(string groupId) {
            base.OnGroup(groupId);
            return this;
        }
    }
}