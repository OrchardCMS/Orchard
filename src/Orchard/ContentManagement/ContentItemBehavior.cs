using System;
using ClaySharp;

namespace Orchard.ContentManagement {
    public class ContentItemBehavior : ClayBehavior {
        private readonly IContent _content;

        public ContentItemBehavior(IContent content) {
            _content = content;
        }

        public override object GetMemberMissing(Func<object> proceed, object self, string name) {
            var contentItem = _content.ContentItem;
            foreach (var part in contentItem.Parts) {
                if (part.PartDefinition.Name == name)
                    return part;
            }
            return base.GetMemberMissing(proceed, self, name);
        }
    }
}
