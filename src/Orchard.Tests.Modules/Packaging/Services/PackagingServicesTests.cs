using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Handlers;
using Orchard.Core.Settings.Metadata;
using Orchard.Core.Settings.Models;
using Orchard.Core.Settings.Services;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Security.Providers;
using Orchard.Settings;
using Orchard.Tests.Modules.Users;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;
using Orchard.Users.Handlers;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Tests.Modules.Packaging.Services {
    public class PackagingServicesTests : DatabaseEnabledTestsBase {
        private Mock<IAuthorizer> _authorizer;
        private Mock<WorkContext> _workContext;
        private IUser _currentUser;

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<SiteService>().As<ISiteService>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType(typeof(SettingsFormatter))
                .As(typeof(IMapper<XElement, SettingsDictionary>))
                .As(typeof(IMapper<SettingsDictionary, XElement>));
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>().InstancePerDependency();
            builder.RegisterType<MembershipService>().As<IMembershipService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<UserPartHandler>().As<IContentHandler>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<TransactionManager>().As<ITransactionManager>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<SiteSettingsPartHandler>().As<IContentHandler>();
            builder.RegisterType<PackagingServices>().As<IPackagingServices>();

            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();

            builder.RegisterType<DefaultEncryptionService>().As<IEncryptionService>();
            builder.RegisterInstance(ShellSettingsUtility.CreateEncryptionEnabled());

            _authorizer = new Mock<IAuthorizer>();
            builder.RegisterInstance(_authorizer.Object);

            _workContext = new Mock<WorkContext>();
            _workContext.Setup(w => w.GetState<ISite>(It.Is<string>(s => s == "CurrentSite"))).Returns(() => _container.Resolve<ISiteService>().GetSiteSettings());
            _workContext.Setup(w => w.GetState<IUser>(It.Is<string>(s => s == "CurrentUser"))).Returns(() => _currentUser);

            var _workContextAccessor = new Mock<IWorkContextAccessor>();
            _workContextAccessor.Setup(w => w.GetContext()).Returns(_workContext.Object);
            builder.RegisterInstance(_workContextAccessor.Object).As<IWorkContextAccessor>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] { typeof(UserPartRecord),
                    typeof(SiteSettingsPartRecord), 
                    typeof(RegistrationSettingsPartRecord), 
                    typeof(ContentTypeRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentItemVersionRecord), 
                };
            }
        }

        [Test]
        public void CanManagePackagesTest() {
            const string superUsername = "admin";
            const string regularUsername = "user1";

            IPackagingServices packagingServices = _container.Resolve<IPackagingServices>();
            IOrchardServices orchardServices = _container.Resolve<IOrchardServices>();
            ShellSettings shellSettings = _container.Resolve<ShellSettings>();
            shellSettings.Name = ShellSettings.DefaultName;

            IUser regularUser = CreateUser(regularUsername);
            IUser superUser = CreateUser(superUsername);

            orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>().SuperUser = superUsername;
            _currentUser = regularUser;
            _session.Flush();

            // Setup authorizer to return false
            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(false);

            // Test regular user without permission explicit assigned and which is not the default admin
            Assert.That(packagingServices.CanManagePackages(), Is.False);

            // Test regular user with permission explicit assigned and which is not the default admin
            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(true);
            Assert.That(packagingServices.CanManagePackages(), Is.True);

            // Test super user that even without permission explicit assigned should be able to manage packages
            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(false);
            Assert.That(packagingServices.CanManagePackages(), Is.False);
            _currentUser = superUser;
            Assert.That(packagingServices.CanManagePackages(), Is.True);

            // Test with super user from another tenant site
            shellSettings.Name = "tenantsite2";
            _session.Flush();

            Assert.That(packagingServices.CanManagePackages(), Is.False);
        }

        private IUser CreateUser(string username) {
            var manager = _container.Resolve<IContentManager>();

            UserPart userPart = manager.New<UserPart>("User");
            userPart.Record = new UserPartRecord { UserName = username, NormalizedUserName = username, Email = string.Format("{0}@orcharproject.com", username) };
            manager.Create(userPart.ContentItem);

            return userPart;
        }
    }
}