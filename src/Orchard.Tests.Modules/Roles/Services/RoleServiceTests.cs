using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.Roles.Events;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security.Permissions;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Roles.Services {
    [TestFixture]
    public class RoleServiceTests : DatabaseEnabledTestsBase {
        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<RoleService>().As<IRoleService>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<TestPermissionProvider>().As<IPermissionProvider>();
            builder.RegisterInstance(new Mock<IRoleEventHandler>().Object);
        }

        public class TestPermissionProvider : IPermissionProvider {
            public Feature Feature {
                get { return new Feature { Descriptor = new FeatureDescriptor { Id = "RoleServiceTests" } }; }
            }

            public IEnumerable<Permission> GetPermissions() {
                return "alpha,beta,gamma,delta".Split(',').Select(name => new Permission { Name = name });
            }

            public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
                return Enumerable.Empty<PermissionStereotype>();
            }
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

        [Test]
        public void PermissionChangesShouldBeVisibleImmediately() {

            var service = _container.Resolve<IRoleService>();

            ClearSession();
            {
                service.CreateRole("test");
                var roleId = service.GetRoleByName("test").Id;
                service.UpdateRole(roleId, "test", new[] { "alpha", "beta", "gamma" });
            }

            ClearSession();
            {
                var result = service.GetPermissionsForRoleByName("test");
                Assert.That(result.Count(), Is.EqualTo(3));
            }

            ClearSession();
            {
                var roleId = service.GetRoleByName("test").Id;
                service.UpdateRole(roleId, "test", new[] { "alpha", "beta", "gamma", "delta" });
            }

            ClearSession();
            {
                var result = service.GetPermissionsForRoleByName("test");
                Assert.That(result.Count(), Is.EqualTo(4));
            }
        }

        [Test]
        public void ShouldNotCreateARoleTwice() {
            var service = _container.Resolve<IRoleService>();
            service.CreateRole("one");
            service.CreateRole("two");
            service.CreateRole("one");

            ClearSession();

            var roles = service.GetRoles();
            Assert.That(roles.Count(), Is.EqualTo(2));
            Assert.That(roles, Has.Some.Property("Name").EqualTo("one"));
            Assert.That(roles, Has.Some.Property("Name").EqualTo("two"));
        }
    }
}
