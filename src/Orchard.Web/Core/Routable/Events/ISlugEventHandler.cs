using Orchard.Events;

namespace Orchard.Core.Routable.Events {
    public interface ISlugEventHandler : IEventHandler {
        void FillingSlugFromTitle(FillSlugContext context);
        void FilledSlugFromTitle(FillSlugContext context);
    }

    public class FillSlugContext {
        public FillSlugContext(string slug) {
            Slug = slug;
        }

        public string Slug { get; set; }
        public bool Adjusted { get; set; }
    }
}