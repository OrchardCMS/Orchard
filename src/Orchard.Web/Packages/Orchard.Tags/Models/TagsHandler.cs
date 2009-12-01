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
            CurrentTags = new List<Tag>();
        }

        public IList<Tag> AllTags { get; set; }
        public IList<Tag> CurrentTags { get; set; }
    }

    public class HasTagsProvider : ContentProvider {
        private readonly IRepository<Tag> _tagsRepository;
        private readonly IRepository<TagsContentItems> _tagsContentItemsRepository;

        public HasTagsProvider(IRepository<Tag> tagsRepository, IRepository<TagsContentItems> tagsContentItemsRepository) {
            _tagsRepository = tagsRepository;
            _tagsContentItemsRepository = tagsContentItemsRepository;
            Filters.Add(new ActivatingFilter<HasTags>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasTags>("blogpost"));

            OnGetDisplays<HasTags>((context, part) => {
                context.Displays.Add(new ModelTemplate(context.ContentItem.Get<HasTags>()) { Position = "2", TemplateName = "HasTagsList" });
                context.Displays.Add(new ModelTemplate(context.ContentItem.Get<HasTags>()) { Position = "5" });
            });
        }


        protected override void Loading(LoadContentContext context) {
            if (context.ContentItem.Has<HasTags>() == false) {
                return;
            }

            HasTags tags = context.ContentItem.Get<HasTags>();
            tags.AllTags = _tagsRepository.Table.ToList();
            IEnumerable<TagsContentItems> tagsContentItems = _tagsContentItemsRepository.Fetch(x => x.ContentItemId == context.ContentItem.Id);
            foreach (var tagContentItem in tagsContentItems) {
                Tag tag = _tagsRepository.Get(tagContentItem.TagId);
                tags.CurrentTags.Add(tag);
            }
        }
    }
}
