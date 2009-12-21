using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Moq;
using NUnit.Framework;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Providers;
using Orchard.Core.Common.Records;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Security;
using Orchard.Tests.Packages;

namespace Orchard.Core.Tests.Common.Providers {
    [TestFixture]
    public class CommonAspectProviderTests : DatabaseEnabledTestsBase {
        private Mock<IAuthenticationService> _authn;
        private Mock<IAuthorizationService> _authz;
        private Mock<IMembershipService> _membership;

        public override void Register(ContainerBuilder builder) {
            builder.Register<DefaultContentManager>().As<IContentManager>();
            builder.Register<TestHandler>().As<IContentHandler>();
            builder.Register<CommonAspectHandler>().As<IContentHandler>();

            _authn = new Mock<IAuthenticationService>();
            _authz = new Mock<IAuthorizationService>();
            _membership = new Mock<IMembershipService>();
            builder.Register(_authn.Object);
            builder.Register(_authz.Object);
            builder.Register(_membership.Object);

        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {typeof (ContentTypeRecord), typeof (ContentItemRecord), typeof (CommonRecord)};
            }
        }

        class TestHandler : ContentHandler {
            public TestHandler() {
                Filters.Add(new ActivatingFilter<CommonAspect>("test-item"));
                Filters.Add(new ActivatingFilter<TestUser>("user"));
            }
        }
        class TestUser : ContentPart, IUser {
            public int Id { get { return 6655321; } }
            public string UserName {get { return "x"; }}
            public string Email { get { return "y"; } }
        }

        [Test]
        public void OwnerShouldBeNullAndZeroByDefault() {
            var contentManager = _container.Resolve<IContentManager>();
            var item = contentManager.Create<CommonAspect>("test-item", init => { });
            ClearSession();

            Assert.That(item.Owner, Is.Null);
            Assert.That(item.Record.OwnerId, Is.EqualTo(0));
        }

        [Test,Ignore("This testing is still being worked out")]
        public void OwnerShouldBeAuthenticatedUserIfAvailable() {
            var contentManager = _container.Resolve<IContentManager>();

            var user = contentManager.New<IUser>("user");
            _authn.Setup(x => x.GetAuthenticatedUser()).Returns(user);

            var item = contentManager.Create<CommonAspect>("test-item", init => { });
            
            ClearSession();

            Assert.That(item.Record.OwnerId, Is.EqualTo(6655321));
        }
    }
}
