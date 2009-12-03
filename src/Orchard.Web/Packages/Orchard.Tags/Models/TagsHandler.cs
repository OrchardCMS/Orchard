using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.ViewModels;
using Orchard.Tags.Services;

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
        private readonly ITagService _tagService;

        public HasTagsProvider(IRepository<Tag> tagsRepository, IRepository<TagsContentItems> tagsContentItemsRepository, ITagService tagService) {
            _tagsRepository = tagsRepository;
            _tagsContentItemsRepository = tagsContentItemsRepository;
            _tagService = tagService;
            Filters.Add(new ActivatingFilter<HasTags>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasTags>("blogpost"));

            OnGetDisplayViewModel<HasTags>((context, hasTags) => {
                context.AddDisplay(new TemplateViewModel(hasTags) { ZoneName="metatop", Position = "2",  TemplateName = "HasTagsList" });
                context.AddDisplay(new TemplateViewModel(hasTags) { ZoneName = "metabottom", Position = "5" });
            });
        }

        protected override void GetEditorViewModel(GetEditorViewModelContext context) {
            if (context.ContentItem.Has<HasTags>() == false) {
                return;
            }
            context.AddEditor(new TemplateViewModel(context.ContentItem.Get<HasTags>()));
        }

        protected override void UpdateEditorViewModel(UpdateEditorViewModelContext context) {
            if (context.ContentItem.Has<HasTags>() == false) {
                return;
            }

            TagsViewModel viewModel = new TagsViewModel();
            context.Updater.TryUpdateModel(viewModel, String.Empty, null, null);
            List<string> tagNames = ParseCommaSeparatedTagNames(viewModel.Tags);
            _tagService.UpdateTagsForContentItem(context.ContentItem.Id, tagNames);

            context.AddEditor(new TemplateViewModel(context.ContentItem.Get<HasTags>()));
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

        private static List<string> ParseCommaSeparatedTagNames(string tags) {
            if (String.IsNullOrEmpty(tags)) {
                return new List<string>();
            }
            IEnumerable<string> tagNames = tags.Split(',');
            List<string> sanitizedTagNames = new List<string>();
            foreach (var tagName in tagNames) {
                if (!String.IsNullOrEmpty(tagName)) {
                    sanitizedTagNames.Add(tagName);
                }
            }
            return sanitizedTagNames;
        }

        public class TagsViewModel {
            public string Tags { get; set; }
        }
    }
}
