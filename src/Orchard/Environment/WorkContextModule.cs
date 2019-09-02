using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Module = Autofac.Module;

namespace Orchard.Environment {
    public class WorkContextModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<WorkContextAccessor>()
                .As<IWorkContextAccessor>()
                .InstancePerMatchingLifetimeScope("shell");

            builder.Register(ctx => new WorkContextImplementation(ctx.Resolve<IComponentContext>()))
                .As<WorkContext>()
                .InstancePerMatchingLifetimeScope("work");

            builder.RegisterGeneric(typeof(WorkValues<>))
                .InstancePerMatchingLifetimeScope("work");

            builder.RegisterSource(new WorkRegistrationSource());
        }
    }

    public class Work<T> where T : class {
        private readonly Func<Work<T>, T> _resolve;

        public Work(Func<Work<T>, T> resolve) {
            _resolve = resolve;
        }

        public T Value {
            get { return _resolve(this); }
        }
    }


    class WorkValues<T> where T : class {
        public WorkValues(IComponentContext componentContext) {
            ComponentContext = componentContext;
            Values = new Dictionary<Work<T>, T>();
        }

        public IComponentContext ComponentContext { get; private set; }
        public IDictionary<Work<T>, T> Values { get; private set; }
    }

    /// <summary>
    /// Support the <see cref="Meta{T}"/>
    /// types automatically whenever type T is registered with the container.
    /// Metadata values come from the component registration's metadata.
    /// </summary>
    class WorkRegistrationSource : IRegistrationSource {
        static readonly MethodInfo CreateMetaRegistrationMethod = typeof(WorkRegistrationSource).GetMethod(
            "CreateMetaRegistration", BindingFlags.Static | BindingFlags.NonPublic);

        private static bool IsClosingTypeOf(Type type, Type openGenericType) {
            return type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor) {
            var swt = service as IServiceWithType;
            if (swt == null || !IsClosingTypeOf(swt.ServiceType, typeof(Work<>)))
                return Enumerable.Empty<IComponentRegistration>();

            var valueType = swt.ServiceType.GetGenericArguments()[0];

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = CreateMetaRegistrationMethod.MakeGenericMethod(valueType);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(null, new object[] { service, v }))
                .Cast<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents {
            get { return true; }
        }

        static IComponentRegistration CreateMetaRegistration<T>(Service providedService, IComponentRegistration valueRegistration) where T : class {
            var rb = RegistrationBuilder.ForDelegate(
                (c, p) => {
                    var workContextAccessor = c.Resolve<IWorkContextAccessor>();
                    return new Work<T>(w => {
                        var workContext = workContextAccessor.GetContext();
                        if (workContext == null)
                            return default(T);

                        var workValues = workContext.Resolve<WorkValues<T>>();

                        T value;
                        if (!workValues.Values.TryGetValue(w, out value)) {
                            value = (T)workValues.ComponentContext.ResolveComponent(valueRegistration, p);
                            workValues.Values[w] = value;
                        }
                        return value;
                    });
                })
                .As(providedService)
                .Targeting(valueRegistration);

            return rb.CreateRegistration();
        }
    }
}
