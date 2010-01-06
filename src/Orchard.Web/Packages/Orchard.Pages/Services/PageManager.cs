using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Pages.Models;
using Orchard.Pages.Services.Templates;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Utility;

namespace Orchard.Pages.Services {

    public enum PublishHistory {
        Default,
        Discard,
        Preserve
    }

    public class PublishOptions {
        public PublishHistory History { get; set; }
    }

    public class CreatePageParams {
        public CreatePageParams() { }
        public CreatePageParams(string title, string slug, string templateName) {
            Title = title;
            Slug = slug;
            TemplateName = templateName;
        }

        public string Title { get; set; }
        public string Slug { get; set; }
        public string TemplateName { get; set; }
    }

    public interface IPageManager : IDependency {
        IEnumerable<string> GetCurrentlyPublishedSlugs();
        PageRevision GetPublishedBySlug(string slug);

        PageRevision GetLastRevision(int pageId);

        PageRevision CreatePage(CreatePageParams createPageParams);
        PageRevision AcquireDraft(int pageId);
        void ApplyTemplateName(PageRevision revision, string templateName);
        void Publish(PageRevision revision, [NotNull] PublishOptions options);
        void DeletePage(Page page);
        void UnpublishPage(Page page);
    }

    public class PageManager : IPageManager {
        private readonly IRepository<Page> _pageRepository;
        private readonly IRepository<PageRevision> _revisionRepository;
        private readonly IRepository<Published> _publishedRepository;
        private readonly IClock _clock;
        private readonly ITemplateProvider _templateProvider;

        public PageManager(
            IClock clock,
            ITemplateProvider templateProvider,
            IRepository<Page> pageRepository,
            IRepository<PageRevision> revisionRepository,
            IRepository<Published> publishedRepository) {
            _clock = clock;
            _templateProvider = templateProvider;
            _pageRepository = pageRepository;
            _revisionRepository = revisionRepository;
            _publishedRepository = publishedRepository;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        public PageRevision CreatePage(CreatePageParams createPageParams) {
            Logger.Information("CreatePage");

            //var templateDescriptor = _templateProvider.Get(pageCreate.TemplateName);

            var page = new Page();
            var revision = new PageRevision {
                Page = page,
                Title = createPageParams.Title,
                Slug = createPageParams.Slug,
                ModifiedDate = _clock.UtcNow,
                Number = 1
            };
            page.Revisions.Add(revision);

            //if (templateDescriptor != null) {
            //    foreach (var zone in templateDescriptor.Zones) {
            //        revision.Contents.Add(new ContentItem { PageRevision = revision, ZoneName = zone });
            //    }
            //}
            ApplyTemplateName(revision, createPageParams.TemplateName);

            _pageRepository.Create(page);

            return revision;
        }

        public void ApplyTemplateName(PageRevision revision, string templateName) {
            revision.TemplateName = templateName;

            var templateDescriptor = _templateProvider.Get(templateName);
            if (templateDescriptor == null) {
                // no template of that name available, no exception or changes required
                return;
            }

            foreach (var zone in templateDescriptor.Zones) {
                string s = zone;
                if (revision.Contents.Any(r => r.ZoneName == s)) {
                    // take no action if content for a zone is already present
                    continue;
                }

                // add an item for the named zone
                revision.Contents.Add(new ContentItem { PageRevision = revision, ZoneName = zone });
            }
        }

        public PageRevision AcquireDraft(int pageId) {
            Logger.Information("AcquireDraft");

            var lastRevision = GetLastRevision(pageId);

            // if the last revision is unpublished, return it as the draft
            if (_publishedRepository.Count(x => x.PageRevision == lastRevision) == 0) {
                // the act of demanding a draft is sufficient to say it's modified
                lastRevision.ModifiedDate = _clock.UtcNow;
                return lastRevision;
            }

            // otherwise create and attach a new revision to be the draft
            var draftRevision = new PageRevision {
                Page = lastRevision.Page,
                Slug = lastRevision.Slug,
                Title = lastRevision.Title,
                Number = lastRevision.Number + 1,   //TODO: Add a method to transactionally increment the revision number at the page level
                TemplateName = lastRevision.TemplateName,
                ModifiedDate = _clock.UtcNow
            };

            lastRevision.Page.Revisions.Add(draftRevision);

            // and initialize it's contents from the immediately prior revision
            foreach (var item in lastRevision.Contents) {
                draftRevision.Contents.Add(new ContentItem {
                    Content = item.Content,
                    PageRevision = draftRevision,
                    ZoneName = item.ZoneName
                });
            }
            return draftRevision;
        }


        public PageRevision GetLastRevision(int pageId) {
            var page = _pageRepository.Get(pageId);
            if (page == null)
                return null;

            return _revisionRepository
                .Fetch(x => x.Page == page, o => o.Desc(x => x.Number), 0, 1)
                .SingleOrDefault();
        }

        public void Publish(PageRevision revisionToPublish, [NotNull] PublishOptions options) {
            Logger.Information("Publish");

            if (options == null) {
                throw new ArgumentNullException("options");
            }

            // locate or create current published record for this page
            var publishedRecord = _publishedRepository.Get(x => x.Page == revisionToPublish.Page) ?? new Published { Page = revisionToPublish.Page };
            if (publishedRecord.PageRevision == revisionToPublish) {
                //TODO: Logging: informational: why are you trying to publish the published revision?!?!
                return;
            }

            bool discardHistory = options.History == PublishHistory.Discard;
            if (discardHistory) {
                publishedRecord.Page.Revisions.Remove(publishedRecord.PageRevision);
            }

            // update the published date on the revision going live
            revisionToPublish.PublishedDate = _clock.UtcNow;

            // alter the current published record to point to the selected revision
            // update the denormalized data
            publishedRecord.PageRevision = revisionToPublish;
            publishedRecord.Slug = revisionToPublish.Slug;

            // add the record if it just came into existence
            if (publishedRecord.Id == 0)
                _publishedRepository.Create(publishedRecord);
        }

        public void DeletePage(Page page) {
            if (page.Published != null) {
                _publishedRepository.Delete(page.Published);
                page.Published = null;
            }
            _pageRepository.Delete(page);
        }

        public void UnpublishPage(Page page) {
            // Remove the published entry
            if (page.Published == null) {
                Logger.Information("Unpublishing an unpublished page is a no-op");
                return;
            }
            _publishedRepository.Delete(page.Published);
            page.Published = null;
        }

        public PageRevision GetPublishedBySlug(string slug) {
            var published = _publishedRepository.Get(x => x.Slug == slug);
            if (published == null)
                return null;
            return published.PageRevision;
        }

        public IEnumerable<string> GetCurrentlyPublishedSlugs() {
            return _publishedRepository.Table.Select(x => x.Slug).ToReadOnlyCollection();
        }
    }
}
