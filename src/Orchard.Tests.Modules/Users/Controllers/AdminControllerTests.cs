using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Environment;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;
using Orchard.Users.Controllers;
using Orchard.Users.Handlers;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;

namespace Orchard.Tests.Modules.Users.Controllers {
    [TestFixture]
    public class AdminControllerTests : DatabaseEnabledTestsBase {
        private AdminController _controller;
        private Mock<IAuthorizer> _authorizer;

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<AdminController>().SingleInstance();
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
            builder.RegisterInstance(new Mock<INotifier>().Object);
            _authorizer = new Mock<IAuthorizer>();
            builder.RegisterInstance(_authorizer.Object);
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] { typeof(UserPartRecord), 
                    typeof(ContentTypeRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentItemVersionRecord), 
                };
            }
        }

        public override void Init() {
            base.Init();

            var manager = _container.Resolve<IContentManager>();

            var userOne = manager.New<UserPart>("User");
            userOne.Record = new UserPartRecord { UserName = "one" };
            manager.Create(userOne.ContentItem);

            var userTwo = manager.New<UserPart>("User");
            userTwo.Record = new UserPartRecord { UserName = "two" };
            manager.Create(userTwo.ContentItem);

            var userThree = manager.New<UserPart>("User");
            userThree.Record = new UserPartRecord { UserName = "three" };
            manager.Create(userThree.ContentItem);

            _controller = _container.Resolve<AdminController>();

            var mockHttpContext = new Mock<HttpContextBase>();
            _controller.ControllerContext = new ControllerContext(
                mockHttpContext.Object,
                new RouteData(
                    new Route("foo", new MvcRouteHandler()),
                    new MvcRouteHandler()),
                _controller);
        }

        [Test]
        public void IndexShouldReturnRowsForUsers() {
            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(true);

            var controller = _container.Resolve<AdminController>();
            var result = (ViewResult)controller.Index();
            var model = (UsersIndexViewModel)result.ViewData.Model;

            Assert.That(model.Rows, Is.Not.Null);
        }


        [Test]
        public void CreateShouldAddUserAndRedirect() {
            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(true);

            var controller = _container.Resolve<AdminController>();
            var result = controller.CreatePOST(new UserCreateViewModel {
                UserName = "four",
                Email = "six@example.org",
                Password = "five",
                ConfirmPassword = "five"
            });
            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

            var redirect = (RedirectToRouteResult)result;
            var id = Convert.ToInt32(redirect.RouteValues["id"]);
            var manager = _container.Resolve<IContentManager>();
            var user = manager.Get(id).As<IUser>();
            Assert.That(user.UserName, Is.EqualTo("four"));
        }

        [Test]
        public void EditShouldDisplayUserAndStoreChanges() {
            _authorizer.Setup(x => x.Authorize(It.IsAny<Permission>(), It.IsAny<LocalizedString>())).Returns(true);

            var repository = _container.Resolve<IRepository<UserPartRecord>>();
            var id = repository.Get(x => x.UserName == "two").Id;
            var result = (ViewResult)_container.Resolve<AdminController>().Edit(id);
            var model = (UserEditViewModel)result.ViewData.Model;
            Assert.That(model.UserName, Is.EqualTo("two"));

            var controller = _container.Resolve<AdminController>();
            controller.ValueProvider = Values.From(new {
                UserName = "bubba",
                Email = "hotep",
            });
            var result2 = controller.EditPOST(id);
            Assert.That(result2, Is.TypeOf<RedirectToRouteResult>());
        }
    }
}
