using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Tags.Models {
    public class HasTags : ContentPart {
        public HasTags() {
            AllTags = new List<Tag>();
        }

        public IEnumerable<Tag> AllTags { get; set; }
    }

    public class HasTagsProvider : ContentProvider {
        private readonly IRepository<Tag> _tagsRepository;

        public HasTagsProvider(IRepository<Tag> tagsRepository) {
            _tagsRepository = tagsRepository;
            Filters.Add(new ActivatingFilter<HasTags>("sandboxpage"));
        }

        protected override void GetDisplays(GetDisplaysContext context) {
            if (context.ContentItem.Has<HasTags>() == false) {
                return;
            }
            context.Displays.Add(new ModelTemplate { Model = context.ContentItem.Get<HasTags>(), Prefix = String.Empty });
        }

        protected override void Loading(LoadContentContext context) {
            if (context.ContentItem.Has<HasTags>() == false) {
                return;
            }

            HasTags tags = context.ContentItem.Get<HasTags>();
            tags.AllTags = _tagsRepository.Table.ToList();
        }
    }
}
