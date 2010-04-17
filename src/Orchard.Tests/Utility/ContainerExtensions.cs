using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Moq;

namespace Orchard.Tests.Utility {
    public static class ContainerExtensions {
        public static Mock<T> Mock<T>(this IContainer container) where T : class {
            return container.Resolve<Mock<T>>();
        }

        public static void RegisterAutoMocking(this ContainerBuilder builder) {
            builder.RegisterSource(new AutoMockSource(MockBehavior.Default));
        }

        public static void RegisterAutoMocking(this ContainerBuilder builder, MockBehavior behavior) {
            builder.RegisterSource(new AutoMockSource(behavior));
        }
        class AutoMockSource : IRegistrationSource {
            private readonly MockBehavior _behavior;

            public AutoMockSource(MockBehavior behavior) {
                _behavior = behavior;
            }

            public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor) {

                var swt = service as IServiceWithType;
                if (swt == null)
                    yield break;
                var st = swt.ServiceType;

                if (st.IsGenericType && st.GetGenericTypeDefinition() == typeof(Mock<>)) {                    
                    yield return RegistrationBuilder.ForType(st)
                        .SingleInstance()
                        .WithParameter("behavior", _behavior)
                        .CreateRegistration();
                }
                else if (st.IsInterface) {
                    yield return RegistrationBuilder.ForDelegate(
                        (ctx, p) => {
                            Trace.WriteLine(string.Format("Mocking {0}", st));
                            var mt = typeof(Mock<>).MakeGenericType(st);
                            var m = (Mock)ctx.Resolve(mt);
                            return m.Object;
                        })
                        .As(service)
                        .SingleInstance()
                        .CreateRegistration();

                }
            }
        }

    }
}