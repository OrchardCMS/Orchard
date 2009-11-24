using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Builder;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users.Controllers;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Users.ViewModels;

namespace Orchard.Tests.Packages.Users.Controllers {
    [TestFixture]
    public class AdminControllerTests : DatabaseEnabledTestsBase {
        private AdminController _controller;

        public override void Register(ContainerBuilder builder) {
            builder.Register<AdminController>();
            builder.Register<DefaultContentManager>().As<IContentManager>();
            builder.Register<MembershipService>().As<IMembershipService>();
            builder.Register<UserProvider>().As<IContentProvider>();
            builder.Register(new Mock<INotifier>().Object);
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] { typeof(UserRecord), typeof(ContentItemRecord), typeof(ContentTypeRecord) };
            }
        }

        public override void Init() {
            base.Init();

            var manager = _container.Resolve<IContentManager>();

            var userOne = manager.New<User>("user");
            userOne.Record = new UserRecord { UserName = "one" };
            manager.Create(userOne.ContentItem);

            var userTwo = manager.New<User>("user");
            userTwo.Record = new UserRecord { UserName = "two" };
            manager.Create(userTwo.ContentItem);

            var userThree = manager.New<User>("user");
            userThree.Record = new UserRecord { UserName = "three" };
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
            var controller = _container.Resolve<AdminController>();
            var result = (ViewResult)controller.Index();
            var model = (UsersIndexViewModel)result.ViewData.Model;

            Assert.That(model.Rows, Is.Not.Null);
        }

        [Test]
        public void CreateShouldAddUserAndRedirect() {
            var controller = _container.Resolve<AdminController>();
            var result = controller.Create(new UserCreateViewModel { UserName = "four",Password="five",ConfirmPassword="five" });
            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());

            var redirect = (RedirectToRouteResult)result;
            var id = Convert.ToInt32(redirect.RouteValues["id"]);
            var manager = _container.Resolve<IContentManager>();
            var user = manager.Get(id).As<IUser>();
            Assert.That(user.UserName, Is.EqualTo("four"));
        }

        [Test]
        public void EditShouldDisplayUserAndStoreChanges() {
            var repository = _container.Resolve<IRepository<UserRecord>>();
            var id = repository.Get(x => x.UserName == "two").Id;
            var result = (ViewResult)_container.Resolve<AdminController>().Edit(id);
            var model = (UserEditViewModel)result.ViewData.Model;
            Assert.That(model.UserName, Is.EqualTo("two"));

            var input = new FormCollection { { "UserName", "bubba" }, { "Email", "hotep" } };
            var result2 = _container.Resolve<AdminController>().Edit(id, input);
            Assert.That(result2, Is.TypeOf<RedirectToRouteResult>());
        }
    }
}
