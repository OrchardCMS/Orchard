using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Roles.Models;
using Orchard.Roles.Services;

namespace Orchard.Tests.Modules.Roles.Services {
    [TestFixture]
    public class RoleServiceTests : DatabaseEnabledTestsBase{
        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<RoleService>().As<IRoleService>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] { typeof(RoleRecord), typeof(PermissionRecord), typeof(RolesPermissionsRecord) };
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
    }
}
