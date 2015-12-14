using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Autofac;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Drivers.Coordinators;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Drivers;
using Orchard.Core.Common.Handlers;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.OwnerEditor;
using Orchard.Core.Common.Services;
using Orchard.Core.Scheduling.Models;
using Orchard.Core.Scheduling.Services;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Tasks.Scheduling;
using Orchard.Tests.DisplayManagement;
using Orchard.Tests.DisplayManagement.Descriptors;
using Orchard.Tests.Modules;
using System.Web.Mvc;
using Orchard.Tests.Stubs;
using Orchard.Themes;
using Orchard.UI.PageClass;

namespace Orchard.Core.Tests.Common.Providers {
    [TestFixture]
    public class CommonPartProviderTests : DatabaseEnabledTestsBase {
        private Mock<IAuthenticationService> _authn;
        private Mock<IAuthorizationService> _authz;
        private Mock<IMembershipService> _membership;
        private Mock<IContentDefinitionManager> _contentDefinitionManager;

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<TestHandler>().As<IContentHandler>();
            builder.RegisterType<CommonPartHandler>().As<IContentHandler>();
            builder.RegisterType<CommonPartDriver>().As<IContentPartDriver>();
            builder.RegisterType<OwnerEditorDriver>().As<IContentPartDriver>();
            builder.RegisterType<ContentPartDriverCoordinator>().As<IContentHandler>();
            builder.RegisterType<CommonService>().As<ICommonService>();
            builder.RegisterType<ScheduledTaskManager>().As<IScheduledTaskManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubThemeService>().As<IThemeManager>();
            builder.RegisterInstance(new Mock<IOrchardServices>().Object);

            builder.RegisterInstance(new RequestContext(new StubHttpContext(), new RouteData()));
            builder.RegisterInstance(new Orchard.Environment.Work<IEnumerable<IShapeTableEventHandler>>(resolve => _container.Resolve<IEnumerable<IShapeTableEventHandler>>())).AsSelf();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            
            // IContentDisplay
            var workContext = new DefaultDisplayManagerTests.TestWorkContext {
                CurrentTheme = new ExtensionDescriptor { Id = "Hello" }
            };
            builder.RegisterInstance<DefaultDisplayManagerTests.TestWorkContextAccessor>(new DefaultDisplayManagerTests.TestWorkContextAccessor(workContext)).As<IWorkContextAccessor>();
            builder.RegisterInstance(new Mock<IPageClassBuilder>().Object); 
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();

            DefaultShapeTableManagerTests.TestShapeProvider.FeatureShapes = new Dictionary<Feature, IEnumerable<string>> {
                { TestFeature(), new[] { "Parts_Common_Owner_Edit" } }
            };

            builder.RegisterType<DefaultShapeTableManagerTests.TestShapeProvider>().As<IShapeTableProvider>()
                .As<DefaultShapeTableManagerTests.TestShapeProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterInstance(new RouteCollection());
            builder.RegisterModule(new ShapeAttributeBindingModule());

            _authn = new Mock<IAuthenticationService>();
            _authz = new Mock<IAuthorizationService>();
            _membership = new Mock<IMembershipService>();
            _contentDefinitionManager = new Mock<IContentDefinitionManager>();

            builder.RegisterInstance(_authn.Object);
            builder.RegisterInstance(_authz.Object);
            builder.RegisterInstance(_membership.Object);
            builder.RegisterInstance(_contentDefinitionManager.Object);

            var virtualPathProviderMock = new Mock<IVirtualPathProvider>();
            virtualPathProviderMock.Setup(a => a.ToAppRelative(It.IsAny<string>())).Returns("~/yadda");

            builder.RegisterInstance(virtualPathProviderMock.Object);
        }

        static Feature TestFeature() {
            return new Feature {
                Descriptor = new FeatureDescriptor {
                    Id = "Testing",
                    Dependencies = Enumerable.Empty<string>(),
                    Extension = new ExtensionDescriptor {
                        Id = "Testing",
                        ExtensionType = DefaultExtensionTypes.Module,
                    }
                }
            };
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(ContentTypeRecord), 
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(CommonPartRecord),
                                 typeof(CommonPartVersionRecord),
                                 typeof(ScheduledTaskRecord),
                             };
            }
        }

        [UsedImplicitly]
        class TestHandler : ContentHandler {
            public TestHandler() {
                Filters.Add(new ActivatingFilter<CommonPart>("test-item"));
                Filters.Add(new ActivatingFilter<ContentPart<CommonPartVersionRecord>>("test-item"));
                Filters.Add(new ActivatingFilter<TestUser>("User"));
            }
        }

        class TestUser : ContentPart, IUser {
            public new int Id { get { return 6655321; } }
            public string UserName { get { return "x"; } }
            public string Email { get { return "y"; } }
        }

        [Test]
        public void OwnerShouldBeNullAndZeroByDefault() {
            var contentManager = _container.Resolve<IContentManager>();
            var item = contentManager.Create<CommonPart>("test-item", init => { });
            ClearSession();

            Assert.That(item.Owner, Is.Null);
            Assert.That(item.Record.OwnerId, Is.EqualTo(0));
        }

        [Test]
        public void PublishingShouldFailIfOwnerIsUnknown() {
            var contentManager = _container.Resolve<IContentManager>();
            var updateModel = new Mock<IUpdateModel>();

            var user = contentManager.New<IUser>("User");
            _authn.Setup(x => x.GetAuthenticatedUser()).Returns(user);

            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });
            var viewModel = new OwnerEditorViewModel { Owner = "User" };
            updateModel.Setup(x => x.TryUpdateModel(viewModel, "", null, null)).Returns(true);
            contentManager.UpdateEditor(item.ContentItem, updateModel.Object);
        }

        class UpdatModelStub : IUpdateModel {

            ModelStateDictionary _modelState = new ModelStateDictionary();

            public ModelStateDictionary ModelErrors {
                get { return _modelState; }
            }

            public string Owner { get; set; }

            public bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
                (model as OwnerEditorViewModel).Owner = Owner;
                return true;
            }

            public void AddModelError(string key, LocalizedString errorMessage) {
                _modelState.AddModelError(key, errorMessage.ToString());
            }
        }

        class StubThemeService : IThemeManager {
            private readonly ExtensionDescriptor _theme = new ExtensionDescriptor {
                Id = "SafeMode",
                Name = "SafeMode",
                Location = "~/Themes",
            };

            public ExtensionDescriptor GetRequestTheme(RequestContext requestContext) { return _theme; }
        }

        [Test]
        public void PublishingShouldNotThrowExceptionIfOwnerIsNull() {
            var contentManager = _container.Resolve<IContentManager>();

            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            var user = contentManager.New<IUser>("User");
            _authn.Setup(x => x.GetAuthenticatedUser()).Returns(user);
            _authz.Setup(x => x.TryCheckAccess(StandardPermissions.SiteOwner, user, item)).Returns(true);

            item.Owner = user;

            var updater = new UpdatModelStub() { Owner = null };

            contentManager.UpdateEditor(item.ContentItem, updater);
        }

        [Test]
        public void PublishingShouldFailIfOwnerIsEmpty() {
            var contentManager = _container.Resolve<IContentManager>();

            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            var user = contentManager.New<IUser>("User");
            _authn.Setup(x => x.GetAuthenticatedUser()).Returns(user);
            _authz.Setup(x => x.TryCheckAccess(StandardPermissions.SiteOwner, user, item)).Returns(true);

            item.Owner = user;

            var updater = new UpdatModelStub() { Owner = "" };

            _container.Resolve<DefaultShapeTableManagerTests.TestShapeProvider>().Discover =
                b => b.Describe("Parts_Common_Owner_Edit").From(TestFeature())
                               .Placement(ctx => new PlacementInfo { Location = "Content" });

            contentManager.UpdateEditor(item.ContentItem, updater);

            Assert.That(updater.ModelErrors.ContainsKey("OwnerEditor.Owner"), Is.True);
        }

        [Test]
        public void PublishingShouldNotFailIfOwnerIsEmptyAndShapeIsHidden() {
            var contentManager = _container.Resolve<IContentManager>();

            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            var user = contentManager.New<IUser>("User");
            _authn.Setup(x => x.GetAuthenticatedUser()).Returns(user);
            _authz.Setup(x => x.TryCheckAccess(StandardPermissions.SiteOwner, user, item)).Returns(true);

            item.Owner = user;

            var updater = new UpdatModelStub() { Owner = "" };

            _container.Resolve<DefaultShapeTableManagerTests.TestShapeProvider>().Discover =
                b => b.Describe("Parts_Common_Owner_Edit").From(TestFeature())
                               .Placement(ctx => new PlacementInfo { Location = "-" });

            contentManager.UpdateEditor(item.ContentItem, updater);

            Assert.That(updater.ModelErrors.ContainsKey("OwnerEditor.Owner"), Is.False);
        }
        [Test]
        public void CreatingShouldSetCreatedAndModifiedUtc() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);
        }

        [Test]
        public void PublishingShouldSetPublishUtcAndShouldNotChangeModifiedUtc() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc = _clock.UtcNow;

            contentManager.Publish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(publishUtc));
        }

        [Test]
        public void PublishingTwiceShouldKeepSettingPublishUtcAndShouldNotChangeModifiedUtc() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc1 = _clock.UtcNow;

            contentManager.Publish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(publishUtc1));

            contentManager.Unpublish(item.ContentItem);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc2 = _clock.UtcNow;

            contentManager.Publish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(publishUtc2));
        }

        [Test]
        public void UnpublishingShouldNotChangePublishUtcAndModifiedUtc() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc = _clock.UtcNow;

            contentManager.Publish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(publishUtc));

            contentManager.Unpublish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(publishUtc));
        }

        [Test]
        public void EditingShouldSetModifiedUtc() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });
            contentManager.Publish(item.ContentItem);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(createUtc));

            _clock.Advance(TimeSpan.FromMinutes(1));
            var editUtc = _clock.UtcNow;

            var updater = new UpdatModelStub() { Owner = "" };
            contentManager.UpdateEditor(item.ContentItem, updater);

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.ModifiedUtc, Is.EqualTo(editUtc));
            Assert.That(item.PublishedUtc, Is.EqualTo(createUtc));
            Assert.That(updater.ModelErrors.Count, Is.EqualTo(0));
        }

        [Test]
        public void VersioningItemShouldCreatedAndPublishedUtcValuesPerVersion() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item1 = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item1.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item1.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publish1Utc = _clock.UtcNow;
            contentManager.Publish(item1.ContentItem);

            // db records need to be updated before demanding draft as item2 below
            _session.Flush();

            _clock.Advance(TimeSpan.FromMinutes(1));
            var draftUtc = _clock.UtcNow;
            var item2 = contentManager.GetDraftRequired<ICommonPart>(item1.ContentItem.Id);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publish2Utc = _clock.UtcNow;
            contentManager.Publish(item2.ContentItem);

            // both instances non-versioned dates show it was created upfront
            Assert.That(item1.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item2.CreatedUtc, Is.EqualTo(createUtc));

            // both instances non-versioned dates show the most recent publish
            Assert.That(item1.PublishedUtc, Is.EqualTo(publish2Utc));
            Assert.That(item2.PublishedUtc, Is.EqualTo(publish2Utc));

            // version1 versioned dates show create was upfront and publish was oldest
            Assert.That(item1.VersionCreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item1.VersionPublishedUtc, Is.EqualTo(publish1Utc));

            // version2 versioned dates show create was midway and publish was most recent
            Assert.That(item2.VersionCreatedUtc, Is.EqualTo(draftUtc));
            Assert.That(item2.VersionPublishedUtc, Is.EqualTo(publish2Utc));
        }

        [Test]
        public void UnpublishShouldClearFlagButLeaveMostrecentPublishDatesIntact() {
            var contentManager = _container.Resolve<IContentManager>();

            var createUtc = _clock.UtcNow;
            var item = contentManager.Create<ICommonPart>("test-item", VersionOptions.Draft, init => { });

            Assert.That(item.CreatedUtc, Is.EqualTo(createUtc));
            Assert.That(item.PublishedUtc, Is.Null);

            _clock.Advance(TimeSpan.FromMinutes(1));
            var publishUtc = _clock.UtcNow;
            contentManager.Publish(item.ContentItem);

            // db records need to be updated before seeking by published flags
            _session.Flush();

            _clock.Advance(TimeSpan.FromMinutes(1));
            var unpublishUtc = _clock.UtcNow;
            contentManager.Unpublish(item.ContentItem);

            // db records need to be updated before seeking by published flags
            ClearSession();

            var publishedItem = contentManager.Get<ICommonPart>(item.ContentItem.Id, VersionOptions.Published);
            var latestItem = contentManager.Get<ICommonPart>(item.ContentItem.Id, VersionOptions.Latest);
            var draftItem = contentManager.Get<ICommonPart>(item.ContentItem.Id, VersionOptions.Draft);
            var allVersions = contentManager.GetAllVersions(item.ContentItem.Id);

            Assert.That(publishedItem, Is.Null);
            Assert.That(latestItem, Is.Not.Null);
            Assert.That(draftItem, Is.Not.Null);
            Assert.That(allVersions.Count(), Is.EqualTo(1));
            Assert.That(publishUtc, Is.Not.EqualTo(unpublishUtc));
            Assert.That(latestItem.PublishedUtc, Is.EqualTo(publishUtc));
            Assert.That(latestItem.VersionPublishedUtc, Is.EqualTo(publishUtc));
            Assert.That(latestItem.ContentItem.VersionRecord.Latest, Is.True);
            Assert.That(latestItem.ContentItem.VersionRecord.Published, Is.False);
        }
    }
}
