using System;
using System.Linq;

namespace Orchard.Models.Driver {
    public class ActivatingFilter<TPart> : IContentActivatingFilter where TPart : class, IContentItemPart, new() {
        private readonly Func<string, bool> _predicate;

        public ActivatingFilter(Func<string, bool> predicate) {
            _predicate = predicate;
        }

        public ActivatingFilter(params string[] contentTypes)
            : this(contentType => contentTypes.Contains(contentType)) {
        }

        public void Activating(ActivatingContentContext context) {
            if (_predicate(context.ContentType))
                context.Builder.Weld<TPart>();
        }
    }

}
