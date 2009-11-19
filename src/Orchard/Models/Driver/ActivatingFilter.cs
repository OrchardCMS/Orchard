using System;
using System.Linq;

namespace Orchard.Models.Driver {
    public class ActivatingFilter<TPart> : IModelActivatingFilter where TPart : class, IContentItemPart, new() {
        private readonly Func<string, bool> _predicate;

        public ActivatingFilter(Func<string, bool> predicate) {
            _predicate = predicate;
        }

        public ActivatingFilter(params string[] modelTypes)
            : this(modelType => modelTypes.Contains(modelType)) {
        }

        public void Activating(ActivatingModelContext context) {
            if (_predicate(context.ModelType))
                context.Builder.Weld<TPart>();
        }
    }

}
