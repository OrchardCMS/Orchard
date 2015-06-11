using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Moq;

namespace Orchard.Tests.Utility {
    public static class ContainerExtensions {
        public static Mock<T> Mock<T>(this IComponentContext container) where T : class {
            return container.Resolve<Mock<T>>();
        }

        public static AutoMockSource RegisterAutoMocking(this ContainerBuilder builder) {
            var source = new AutoMockSource(MockBehavior.Strict);
            builder.RegisterSource(source);
            return source;
        }

        public static void RegisterAutoMocking(this ContainerBuilder builder, MockBehavior behavior) {
            builder.RegisterSource(new AutoMockSource(behavior));
        }

        public class AutoMockSource : IRegistrationSource {
            private readonly MockBehavior _behavior;
            private IEnumerable<Type> _ignore = Enumerable.Empty<Type>();

            public AutoMockSource(MockBehavior behavior) {
                _behavior = behavior;
                Ignore<IStartable>();
            }
            
            public bool IsAdapterForIndividualComponents {
                get { return false; }
            }

            IEnumerable<IComponentRegistration> IRegistrationSource.RegistrationsFor(
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
                    if (st.IsGenericType && st.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                        yield break;
                    }
                    if (_ignore.Contains(st)) {
                        yield break;
                    }

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

            public AutoMockSource Ignore<T>() {
                _ignore = _ignore.Concat(new[]{typeof (T)});
                return this;
            }
        }

    }
}