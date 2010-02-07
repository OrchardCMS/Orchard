using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using NUnit.Framework;
using Orchard.Roles.Records;
using Orchard.Roles.Services;

namespace Orchard.Tests.Modules.Roles.Services {
    [TestFixture]
    public class RoleServiceTests : DatabaseEnabledTestsBase{
        public override void Register(ContainerBuilder builder) {
            builder.Register<RoleService>().As<IRoleService>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] { typeof(RoleRecord), typeof(PermissionRecord), typeof(RolesPermissions) };
            }
        }

        [Test]
        public void CreateRoleShouldAddToList() {
            var service = _container.Resolve<IRoleService>();
            service.CreateRole("one");
            service.CreateRole("two");
            service.CreateRole("three");

            ClearSession();

            var roles = service.GetRoles();
            Assert.That(roles.Count(), Is.EqualTo(3));
            Assert.That(roles, Has.Some.Property("Name").EqualTo("one"));
            Assert.That(roles, Has.Some.Property("Name").EqualTo("two"));
            Assert.That(roles, Has.Some.Property("Name").EqualTo("three"));
        }

        [Test, Ignore("Permissions should be created first it appears?")]
        public void PermissionRecordsShouldBeCreatedOnDemand() {
            var service = _container.Resolve<IRoleService>();
            service.CreateRole("one");
            service.CreatePermissionForRole("one", "foo");
            service.CreatePermissionForRole("one", "bar");
            ClearSession();

            var one = service.GetRoles().Single(x => x.Name == "one");
            Assert.That(one.RolesPermissions, Has.Count.EqualTo(2));
            Assert.That(one.RolesPermissions.Select(x => x.Permission.Name), Has.Some.EqualTo("foo"));
            Assert.That(one.RolesPermissions.Select(x => x.Permission.Name), Has.Some.EqualTo("bar"));
        }
    }
}
