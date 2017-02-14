using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentPartCloningDriver<TContent> : ContentPartDriver<TContent>, IContentPartCloningDriver where TContent : ContentPart, new() {

        void IContentPartCloningDriver.Cloning(CloneContentContext context) {
            var originalPart = context.ContentItem.As<TContent>();
            var clonePart = context.CloneContentItem.As<TContent>();
            if (originalPart != null && clonePart != null)
                Cloning(originalPart, clonePart, context);
        }

        void IContentPartCloningDriver.Cloned(CloneContentContext context) {
            var originalPart = context.ContentItem.As<TContent>();
            var clonePart = context.CloneContentItem.As<TContent>();
            if (originalPart != null && clonePart != null)
                Cloned(originalPart, clonePart, context);
        }


        protected virtual void Cloning(TContent originalPart, TContent clonePart, CloneContentContext context) { }

        protected virtual void Cloned(TContent originalPart, TContent clonePart, CloneContentContext context) { }
    }
}
