using System;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.CmsPages.Controllers;
using Orchard.CmsPages.Models;
using Orchard.CmsPages.Services;
using Orchard.CmsPages.Services.Templates;
using Orchard.CmsPages.ViewModels;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;
using Orchard.Utility;

namespace Orchard.Tests.Packages.Pages.Controllers {
    [TestFixture]
    public class AdminControllerTests : DatabaseEnabledTestsBase {
        private AdminController _controller;
        private IPageManager _pageManager;
        private IPageScheduler _pageScheduler;
        private IAuthorizer _authorizer;
        private ITemplateProvider _templateProvider;
        private int _slugPageId;
        private IRepository<Page> _pagesRepository;

        [SetUp]
        public override void Init() {
            base.Init();

            _pagesRepository = _container.Resolve<IRepository<Page>>();
            //_pagesRepository.Create(new Page { Slug = "slug" });
            _pageManager = _container.Resolve<IPageManager>();
            _pageScheduler = _container.Resolve<IPageScheduler>();
            _templateProvider = _container.Resolve<ITemplateProvider>();
            _authorizer = _container.Resolve<IAuthorizer>();
            var page = _pageManager.CreatePage(new PageCreateViewModel { Slug = "slug", Templates = _templateProvider.List() });
            _slugPageId = page.Id;

            _controller = _container.Resolve<AdminController>();
            _controller.ControllerContext = new ControllerContext(new StubHttpContext("~/admin/cmspages"), new RouteData(), _controller);
        }

        public override void Register(Autofac.Builder.ContainerBuilder builder) {
            builder.Register<AdminController>();
            builder.Register<PageManager>().As<IPageManager>();
            builder.Register<PageScheduler>().As<IPageScheduler>();
            builder.Register<Notifier>().As<INotifier>();
            builder.Register(new StubTemplateProvider()).As<ITemplateProvider>();
            builder.Register(new StubAuthorizer()).As<IAuthorizer>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof (Page), typeof (PageRevision), typeof (ContentItem), typeof (Published),
                                 typeof (Scheduled)
                             };
            }
        }

        class StubTemplateProvider : ITemplateProvider {
            public IList<TemplateDescriptor> List() {
                return new List<TemplateDescriptor> {
                                                        new TemplateDescriptor { Name = "twocolumn", Zones = new[] { "content1", "content2" } },
                                                        new TemplateDescriptor { Name = "threecolumn", Zones = new[] { "content1", "content2", "content3" } }
                                                    };
            }

            public TemplateDescriptor Get(string name) {
                if (name == "twocolumn") {
                    return List()[0];
                }
                if (name == "threecolumn") {
                    return List()[1];
                }
                return null;
            }
        }

        class StubAuthorizer: IAuthorizer {
            #region IAuthorizer Members

            public bool Authorize(Permission permission, LocalizedString message) {
                return true;
            }

            #endregion
        }

        class StubLocalizer {
            public static LocalizedString Get(string textHint, params object[] args) {
                var localizedFormat = textHint;
                var localizedText = string.Format(localizedFormat, args);
                return new LocalizedString(localizedText);
            }
        }

        [Test]
        public void CreateShouldReturnViewWithErrorIfSlugIsNull() {
            var input = new FormCollection { { ReflectOn<PageCreateViewModel>.NameOf(m => m.Slug), null } };
            var result = _controller.Create(input);
            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public void CreateShouldReturnEmptyPageCreateViewModel() {
            var result = _controller.Create();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewResult>());

            var model = ((ViewResult)result).ViewData.Model;
            Assert.That(model, Is.TypeOf<PageCreateViewModel>());

            var pageModel = (PageCreateViewModel)model;
            Assert.That(pageModel.TemplateName, Is.EqualTo("twocolumn"));
            Assert.That(pageModel.Templates.Count(), Is.EqualTo(2));
            Assert.That(pageModel.Templates.First().Name, Is.EqualTo("twocolumn"));
            Assert.That(pageModel.Templates.Skip(1).First().Name, Is.EqualTo("threecolumn"));
        }

        [Test]
        public void CreateShouldCreatePageWithSlugAndTemplateAndRedirectToEdit() {

            var pageDoesntExist = _pageManager.GetPublishedBySlug("slug2");

            var input = new FormCollection { 
                                               { ReflectOn<PageCreateViewModel>.NameOf(m => m.Slug), "slug2" }, 
                                               { ReflectOn<PageCreateViewModel>.NameOf(m => m.TemplateName), "threecolumn" } 
                                           };

            var result = _controller.Create(input);

            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
            var redirect = (RedirectToRouteResult)result;
            Assert.That(redirect.RouteValues["action"], Is.EqualTo("Edit"));

            var pageId = Convert.ToInt32(redirect.RouteValues["id"]);

            var pageWasCreated = _pageManager.GetLastRevision(pageId);
            var pageStillNotVisible = _pageManager.GetPublishedBySlug("slug2");

            Assert.That(pageDoesntExist, Is.Null);
            Assert.That(pageWasCreated, Is.Not.Null);
            Assert.That(pageWasCreated.Slug, Is.EqualTo("slug2"));
            Assert.That(pageWasCreated.TemplateName, Is.EqualTo("threecolumn"));
            Assert.That(pageStillNotVisible, Is.Null);
        }

        [Test]
        public void IndexShouldReturnTheListOfFilteredPages() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);
            _pageManager.Publish(revision, new PublishOptions());

            var createPage2 = new PageCreateViewModel { Title = "hello2", Slug = "world2", TemplateName = "twocolumn" };
            var revision2 = _pageManager.CreatePage(createPage2);
            _pageScheduler.AddPublishTask(revision2, _clock.FutureMoment(TimeSpan.FromMinutes(1)));

            ClearSession();

            Assert.That(_pagesRepository.Count(x => true), Is.EqualTo(3));

            // No filter
            {
                var result = _controller.Index(new PageIndexOptions());
                var pages = (PageIndexViewModel)(((ViewResult)result).ViewData.Model);
                Assert.That(pages.Options.Filter, Is.EqualTo(PageIndexFilter.All));
                Assert.That(pages.PageEntries.Count, Is.EqualTo(3));
                Assert.That(
                    pages.PageEntries.Single(p => p.IsPublished && p.Published.PageRevision.Id == revision.Id).Published.
                        PageRevision.Slug, Is.EqualTo("world"));
            }

            // Published
            {
                var result = _controller.Index(new PageIndexOptions { Filter = PageIndexFilter.Published });

                var pages = (PageIndexViewModel)(((ViewResult)result).ViewData.Model);
                Assert.That(pages.Options.Filter, Is.EqualTo(PageIndexFilter.Published));
                Assert.That(pages.PageEntries.Count, Is.EqualTo(1));
                Assert.That(pages.PageEntries[0].Published.PageRevision.Slug, Is.EqualTo("world"));
            }

            // Offline
            {
                var result = _controller.Index(new PageIndexOptions { Filter = PageIndexFilter.Offline });

                var pages = (PageIndexViewModel)(((ViewResult)result).ViewData.Model);
                Assert.That(pages.Options.Filter, Is.EqualTo(PageIndexFilter.Offline));
                Assert.That(pages.PageEntries.Count, Is.EqualTo(2));
                Assert.That(pages.PageEntries.OrderBy(page => page.Page.Id).First().Page.Revisions.Last().Slug, Is.EqualTo("slug"));
                Assert.That(pages.PageEntries.OrderBy(page => page.Page.Id).Skip(1).First().Page.Revisions.Last().Slug, Is.EqualTo("world2"));
            }

            // Scheduled
            {
                var result = _controller.Index(new PageIndexOptions { Filter = PageIndexFilter.Scheduled });

                var pages = (PageIndexViewModel)(((ViewResult)result).ViewData.Model);
                Assert.That(pages.Options.Filter, Is.EqualTo(PageIndexFilter.Scheduled));
                Assert.That(pages.PageEntries.Count, Is.EqualTo(1));
                Assert.That(pages.PageEntries[0].Page.Revisions.Last().Slug, Is.EqualTo("world2"));
            }
        }

        [Test]
        public void IndexPostShouldPerformBulkPublishNow() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);

            // Add a scheduled publish task to make sure it's deleted when bulk "PublishNow" is called
            _pageScheduler.AddPublishTask(revision, _clock.FutureMoment(TimeSpan.FromMinutes(1)));

            // Check database state
            ClearSession();
            var pages = _pagesRepository.Table.ToList();
            Assert.That(pages.Count, Is.EqualTo(2));
            Assert.That(pages[0].Published, Is.Null);
            Assert.That(pages[1].Published, Is.Null);
            Assert.That(_pagesRepository.Get(revision.Page.Id).Scheduled.Count, Is.EqualTo(1));

            // Build controller input
            var input = new FormCollection { { ReflectOn<PageIndexViewModel>.NameOf(m => m.Options.BulkAction), PageIndexBulkAction.PublishNow.ToString() } };
            for (int i = 0; i < 2; i++) {
                //TODO: Use "NameOf" when it supports these expressions
                input.Add(string.Format("PageEntries[{0}].PageId", i), pages[i].Id.ToString());
                input.Add(string.Format("PageEntries[{0}].IsChecked", i), true.ToString());
            }

            // Call controller
            var result = _controller.Index(input);

            // Verify result, check database state
            ClearSession();
            pages = _pagesRepository.Table.ToList();
            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
            Assert.That(pages[0].Published, Is.Not.Null);
            Assert.That(pages[1].Published, Is.Not.Null);
            Assert.That(pages[0].Scheduled.Count, Is.EqualTo(0));
            Assert.That(pages[1].Scheduled.Count, Is.EqualTo(0));
        }

        [Test]
        public void IndexPostShouldPerformBulkPublishLater() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);

            // Add a scheduled publish task to make sure it's deleted when bulk "PublishNow" is called
            _pageScheduler.AddPublishTask(revision, _clock.FutureMoment(TimeSpan.FromMinutes(1)));

            // Check database state
            ClearSession();
            var pages = _pagesRepository.Table.ToList();
            Assert.That(pages.Count, Is.EqualTo(2));
            Assert.That(pages[0].Published, Is.Null);
            Assert.That(pages[1].Published, Is.Null);
            Assert.That(_pagesRepository.Get(revision.Page.Id).Scheduled.Count, Is.EqualTo(1));

            // Build controller input
            DateTime scheduledDate = _clock.FutureMoment(TimeSpan.FromMinutes(1));

            var input = new FormCollection {
                                               { ReflectOn<PageIndexViewModel>.NameOf(m => m.Options.BulkAction), PageIndexBulkAction.PublishLater.ToString() } ,
                                               { ReflectOn<PageIndexViewModel>.NameOf(m => m.Options.BulkPublishLaterDate), string.Format("{0:d} {0:T}", scheduledDate) } 
                                           };
            for (int i = 0; i < 2; i++) {
                //TODO: Use "NameOf" when it supports these expressions
                input.Add(string.Format("PageEntries[{0}].PageId", i), pages[i].Id.ToString());
                input.Add(string.Format("PageEntries[{0}].IsChecked", i), true.ToString());
            }

            // Call controller
            var result = _controller.Index(input);

            // Verify result, check database state
            ClearSession();
            pages = _pagesRepository.Table.ToList();
            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
            Assert.That(pages[0].Published, Is.Null);
            Assert.That(pages[1].Published, Is.Null);
            Assert.That(pages[0].Scheduled.Count, Is.EqualTo(1));
            Assert.That(pages[0].Scheduled[0].ScheduledDate, Is.EqualTo(scheduledDate));
            Assert.That(pages[1].Scheduled.Count, Is.EqualTo(1));
            Assert.That(pages[1].Scheduled[0].ScheduledDate, Is.EqualTo(scheduledDate));
        }

        [Test]
        public void IndexPostShouldPerformBulkDelete() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);

            // Add a scheduled publish task to make sure it's deleted when bulk "PublishNow" is called
            _pageScheduler.AddPublishTask(revision, _clock.FutureMoment(TimeSpan.FromMinutes(1)));

            // Check database state
            ClearSession();
            var pages = _pagesRepository.Table.ToList();
            Assert.That(pages.Count, Is.EqualTo(2));
            Assert.That(pages[0].Published, Is.Null);
            Assert.That(pages[1].Published, Is.Null);
            Assert.That(_pagesRepository.Get(revision.Page.Id).Scheduled.Count, Is.EqualTo(1));

            // Build controller input
            var input = new FormCollection { 
                                               { ReflectOn<PageIndexViewModel>.NameOf(m => m.Options.BulkAction), PageIndexBulkAction.Delete.ToString() },
                                               { ReflectOn<PageIndexViewModel>.NameOf(m => m.Options.BulkDeleteConfirmed), true.ToString() }
                                           };

            for (int i = 0; i < 2; i++) {
                //TODO: Use "NameOf" when it supports these expressions
                input.Add(string.Format("PageEntries[{0}].PageId", i), pages[i].Id.ToString());
                input.Add(string.Format("PageEntries[{0}].IsChecked", i), true.ToString());
            }

            // Call controller
            var result = _controller.Index(input);

            // Verify result, check database state
            ClearSession();
            _pagesRepository.Table.ToList();
            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
        }

        [Test]
        public void IndexPostShouldPerformBulkUnpublish() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);
            _pageManager.Publish(revision, new PublishOptions());

            // Check database state
            ClearSession();
            var pages = _pagesRepository.Table.ToList();
            Assert.That(pages.Count, Is.EqualTo(2));
            Assert.That(pages[0].Published, Is.Null);
            Assert.That(pages[1].Published, Is.Not.Null);

            // Build controller input
            var input = new FormCollection { 
                                               { ReflectOn<PageIndexViewModel>.NameOf(m => m.Options.BulkAction), PageIndexBulkAction.Unpublish.ToString() },
                                           };

            for (int i = 0; i < 2; i++) {
                //TODO: Use "NameOf" when it supports these expressions
                input.Add(string.Format("PageEntries[{0}].PageId", i), pages[i].Id.ToString());
                input.Add(string.Format("PageEntries[{0}].IsChecked", i), true.ToString());
            }

            // Call controller
            var result = _controller.Index(input);

            // Verify result, check database state
            ClearSession();
            pages = _pagesRepository.Table.ToList();
            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
            Assert.That(pages.Count, Is.EqualTo(2));
            Assert.That(pages[0].Published, Is.Null);
            Assert.That(pages[1].Published, Is.Null);
        }

        [Test]
        [ExpectedException(typeof(HttpException))]
        public void EditShouldThrow404IfSlugIsNotFound() {
            _controller.Edit(6655321);
        }

        [Test]
        public void PublishNowShouldApplyChangesAndRedirect() {
            var pageBeforeEdit = _pageManager.GetLastRevision(_slugPageId);

            var input = new FormCollection { 
                                               { ReflectOn<PageEditViewModel>.NameOf(m => m.Revision.Slug), "new-slug-value" },
                                               { ReflectOn<PageEditViewModel>.NameOf(m => m.Command), PageEditCommand.PublishNow.ToString() }
                                           };
            var result = _controller.Edit(_slugPageId, input);

            var pageNotFoundAnymore = _pageManager.GetPublishedBySlug("slug");
            var pageFromNewSlug = _pageManager.GetPublishedBySlug("new-slug-value");


            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
            Assert.That(pageBeforeEdit, Is.Not.Null);
            Assert.That(pageNotFoundAnymore, Is.Null);
            Assert.That(pageFromNewSlug, Is.Not.Null);
            Assert.That(pageBeforeEdit.Id, Is.EqualTo(pageFromNewSlug.Id));
            Assert.That(pageFromNewSlug.Slug, Is.EqualTo("new-slug-value"));
        }

        [Test]
        public void ChooseTemplateListsAvailableTemplatesWithCurrentOneSelected() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);
            var result = _controller.ChooseTemplate(revision.Id);

            var viewModel = (ChooseTemplateViewModel)((ViewResult)result).ViewData.Model;
            Assert.That(viewModel.TemplateName, Is.EqualTo("twocolumn"));
            Assert.That(viewModel.Templates, Has.Some.Property("Name").EqualTo("twocolumn"));
            Assert.That(viewModel.Templates, Has.Some.Property("Name").EqualTo("threecolumn"));
        }


        [Test]
        public void PostingDifferentTemplateResultsInDraftAndExtendsNamedContentItems() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);
            _pageManager.Publish(revision, new PublishOptions());

            Assert.That(revision.Contents, Has.Count.EqualTo(2));
            Assert.That(revision.Contents, Has.None.Property("ZoneName").EqualTo("content3"));

            var input = new FormCollection { { "TemplateName", "threecolumn" } };
            var result = _controller.ChooseTemplate(revision.Id, input);

            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

            _session.Flush();
            _session.Clear();

            var published = _pageManager.GetPublishedBySlug("world");
            var draft = _pageManager.GetLastRevision(revision.Page.Id);

            // different revision for draft
            Assert.That(draft.Page.Id, Is.EqualTo(published.Page.Id));
            Assert.That(draft.Id, Is.Not.EqualTo(published.Id));

            // content item added
            Assert.That(draft.Contents, Has.Count.EqualTo(3));
            Assert.That(draft.Contents, Has.Some.Property("ZoneName").EqualTo("content3"));
        }

        [Test, Ignore("This can't be properly implementated until a transaction scope with rollback abilities is available to the controller")]
        public void PostingSameTemplateDoesNotResultInDraftBeingCreated() {
            var createPage = new PageCreateViewModel { Title = "hello", Slug = "world", TemplateName = "twocolumn" };
            var revision = _pageManager.CreatePage(createPage);
            _pageManager.Publish(revision, new PublishOptions());

            Assert.That(revision.Contents, Has.Count.EqualTo(2));
            Assert.That(revision.Contents, Has.None.Property("ZoneName").EqualTo("content3"));

            var input = new FormCollection { { "TemplateName", "twocolumn" } };
            var result = _controller.ChooseTemplate(revision.Id, input);

            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

            _session.Flush();
            _session.Clear();


            var published = _pageManager.GetPublishedBySlug("world");
            var draft = _pageManager.GetLastRevision(revision.Page.Id);
            Assert.That(draft.Id, Is.EqualTo(published.Id));

        }

        [Test, Ignore("This actually requires the data binder to be registered, because it's going through contoller's update model method.")]
        public void SavingDraftAfterEmptyingUnusedContentItemShouldRemoveUnusedItems() {
            var createInput = new FormCollection {
                                                     {"Title", "One"},
                                                     {"Slug", "Two"},
                                                     {"TemplateName", "threecolumn"},
                                                 };
            var createResult = (RedirectToRouteResult)_controller.Create(createInput);
            ClearSession();
            var pageId = (int)createResult.RouteValues["id"];

            var publishInput = new FormCollection {
                                                      {"Command", "PublishNow"},
                                                      {"Revision.Contents[content1].Content", "alpha"},
                                                      {"Revision.Contents[content2].Content", "beta"},
                                                      {"Revision.Contents[content3].Content", "gamma"},
                                                  };
            _controller.Edit(pageId, publishInput);
            ClearSession();

            var chooseTemplateInput = new FormCollection {
                                                             {"TemplateName", "twocolumn"},
                                                         };
            _controller.ChooseTemplate(pageId, chooseTemplateInput);
            ClearSession();

            var revision = _pageManager.GetLastRevision(pageId);
            Assert.That(revision.Contents, Has.Count.EqualTo(3));


            var publishInput2 = new FormCollection {
                                                       {"Command", "PublishNow"},
                                                       {"Revision.Contents[content1].Content", "alpha"},
                                                       {"Revision.Contents[content2].Content", "beta"},
                                                       {"Revision.Contents[content3].Content", ""},
                                                   };
            _controller.Edit(pageId, publishInput2);
            ClearSession();
            var revision2 = _pageManager.GetLastRevision(pageId);
            Assert.That(revision2.Contents, Has.Count.EqualTo(2));
        }
    }
}