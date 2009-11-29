using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Autofac.Builder;
using NUnit.Framework;
using Orchard.CmsPages.Controllers;
using Orchard.CmsPages.Services;
using Orchard.CmsPages.Services.Templates;
using Orchard.CmsPages.ViewModels;
using Orchard.CmsPages.Models;
using System.Web;
using Orchard.Tests.Packages.Pages.Services.Templates;

namespace Orchard.Tests.Packages.Pages.Controllers {
    [TestFixture]
    public class TemplatesControllerTests : DatabaseEnabledTestsBase {
        private TemplatesController _controller;
        private IPageManager _pageManager;
        private ITemplateProvider _templateProvider;

        public override void Init() {
            base.Init();

            _pageManager = _container.Resolve<IPageManager>();
            _templateProvider = _container.Resolve<ITemplateProvider>();
            _controller = _container.Resolve<TemplatesController>();

            var revision = _pageManager.CreatePage(new CreatePageParams(null, "slug", null));
            _pageManager.Publish(revision, new PublishOptions());

            _container.Resolve<ISlugConstraint>().SetCurrentlyPublishedSlugs(_pageManager.GetCurrentlyPublishedSlugs());
        }

        public override void Register(ContainerBuilder builder) {
            builder.Register<TemplatesController>();
            builder.Register<PageManager>().As<IPageManager>();
            builder.Register<TemplateProvider>().As<ITemplateProvider>();
            builder.Register<TemplateMetadataParser>().As<ITemplateMetadataParser>();
            builder.Register(new StubTemplateEntryProvider()).As<ITemplateEntryProvider>();
            builder.Register<SlugConstraint>().As<ISlugConstraint>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof (Page), typeof (PageRevision), typeof (ContentItem), typeof (Published),
                                 typeof (Scheduled)
                             };
            }
        }

        [Test]
        [ExpectedException(typeof (HttpException))]
        public void ShowShouldThrow404IfSlugIsNotFound() {
            _controller.Show("notExisting");
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShowShouldThrowIfSlugIsNull() {
            _controller.Show(null);
        }

        [Test]
        public void ShowShouldReturnAViewResult() {
            Assert.That(_controller.Show("slug"), Is.Not.Null);
        }

        [Test]
        public void SlugShouldBeUsedToGetPageFromRepository() {
            var result = _controller.Show("slug");
            var page = (PageRevision) (((ViewResult) result).ViewData.Model);
            Assert.That(page.Slug, Is.EqualTo("slug"));
        }


        [Test]
        public void TheWrongCaseShouldStillWork() {
            var result = _controller.Show("sLUg");
            var page = (PageRevision)(((ViewResult)result).ViewData.Model);
            Assert.That(page.Slug, Is.EqualTo("slug"));
        }
    }
}