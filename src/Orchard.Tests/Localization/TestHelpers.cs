using System;
using System.Web;
using Autofac;
using Moq;
using Orchard.Localization.Services;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Localization {

    internal class TestHelpers {
        public static IContainer InitializeContainer(string cultureName, string calendarName, TimeZoneInfo timeZone, IClock clock = null) {
            var builder = new ContainerBuilder();
            if (clock != null)
                builder.RegisterInstance(clock);
            else
                builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterInstance<WorkContext>(new StubWorkContext(cultureName, calendarName, timeZone));
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<CultureDateTimeFormatProvider>().As<IDateTimeFormatProvider>();
            builder.RegisterType<DefaultDateFormatter>().As<IDateFormatter>();
            builder.RegisterInstance(new Mock<ICalendarSelector>().Object);
            builder.RegisterType<DefaultCalendarManager>().As<ICalendarManager>();
            builder.RegisterType<DefaultDateLocalizationServices>().As<IDateLocalizationServices>();
            return builder.Build();
        }
    }

    internal class StubWorkContext : WorkContext {

        public StubWorkContext() {
        }

        public StubWorkContext(string cultureName, string calendarName, TimeZoneInfo timeZone) {
            CultureName = cultureName;
            CalendarName = calendarName;
            TimeZone = timeZone;
        }

        public string CultureName { get; set; }
        public string CalendarName { get; set; }
        public TimeZoneInfo TimeZone { get; set; }

        public override T Resolve<T>() {
            throw new NotImplementedException();
        }

        public override object Resolve(Type serviceType) {
            throw new NotImplementedException();
        }

        public override bool TryResolve<T>(out T service) {
            throw new NotImplementedException();
        }

        public override bool TryResolve(Type serviceType, out object service) {
            throw new NotImplementedException();
        }

        public override T GetState<T>(string name) {
            if (name == "CurrentCulture") return (T)((object)CultureName);
            if (name == "CurrentCalendar") return (T)((object)CalendarName);
            if (name == "CurrentTimeZone") return (T)((object)TimeZone);
            throw new NotImplementedException(String.Format("Property '{0}' is not implemented.", name));
        }

        public override void SetState<T>(string name, T value) {
            throw new NotImplementedException();
        }
    }

    internal class StubWorkContextAccessor : IWorkContextAccessor {

        private readonly WorkContext _workContext;

        public StubWorkContextAccessor(WorkContext workContext) {
            _workContext = workContext;
        }

        public WorkContext GetContext(HttpContextBase httpContext) {
            throw new NotImplementedException();
        }

        public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
            throw new NotImplementedException();
        }

        public WorkContext GetContext() {
            return _workContext;
        }

        public IWorkContextScope CreateWorkContextScope() {
            throw new NotImplementedException();
        }
    }
}
