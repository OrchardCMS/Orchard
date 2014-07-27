using System;
using System.Globalization;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Framework.Localization.Models;
using Orchard.Framework.Localization.Services;
using Orchard.Localization.Services;

namespace Orchard.Framework.Tests.Localization {

    [TestFixture]
    public class DefaultDateFormatterTests {

        [Test]
        [Description("Correct en-US date is parsed correctly.")]
        public void ParseTest01() {
            var container = InitializeContainer("en-US");
            var culture = CultureInfo.GetCultureInfo("en-US");
            var formats = container.Resolve<IDateTimeFormatProvider>();
            var target = container.Resolve<IDateFormatter>();

            var value = new DateTime(2014, 5, 31, 10, 0, 0).ToString(formats.ShortDateTimeFormat, culture);
            var result = target.ParseDateTime(value);
            var expected = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);
            
            Assert.AreEqual(expected, result);
        }

        [Test]
        [Description("Incorrect en-US date yields an exception.")]
        [ExpectedException(typeof(FormatException))]
        public void ParseTest02() {
            var container = InitializeContainer("en-US");
            var target = container.Resolve<IDateFormatter>();

            var value = "BlaBlaBla";
            var result = target.ParseDateTime(value);
        }

        [Test]
        [Description("Correct sv-SE date is parsed correctly.")]
        public void ParseTest03() {
            var container = InitializeContainer("sv-SE");
            var culture = CultureInfo.GetCultureInfo("sv-SE");
            var formats = container.Resolve<IDateTimeFormatProvider>();
            var target = container.Resolve<IDateFormatter>();

            var value = new DateTime(2014, 5, 31, 10, 0, 0).ToString(formats.ShortDateTimeFormat, culture);
            var result = target.ParseDateTime(value);
            var expected = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Description("Incorrect sv-SE date yields an exception.")]
        [ExpectedException(typeof(FormatException))]
        public void ParseTest04() {
            var container = InitializeContainer("sv-SE");
            var target = container.Resolve<IDateFormatter>();

            var value = "BlaBlaBla";
            var result = target.ParseDateTime(value);
        }

        //[Test]
        //[Description("Loop through all cultures. Test Parse method by all possible DateTimeFormats.")]
        //public void ParseTest04() {
        //    IDateFormatter target = new DefaultDateFormatter();
        //    var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        //    foreach (CultureInfo cultureInfo in cultures) {
        //        // Due to a bug in .NET 4.5 in combination with updated Upper Sorbian culture in Windows 8.
        //        if (System.Environment.OSVersion.Version.ToString().CompareTo("6.2.0.0") >= 0 && cultureInfo.Name.StartsWith("hsb")) {
        //            continue;
        //        }
        //        DateTime dateTime = new DateTime(2014, 12, 31, 10, 20, 40, 567);
        //        cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
        //        var dateString = dateTime.ToString("G", cultureInfo);
        //        var result = target.ParseDateTime(dateString, cultureInfo);
        //        var millisecond = DateTime.Parse(dateString, cultureInfo.DateTimeFormat).Millisecond;
        //        var expected = new DateTimeParts(2014, 12, 31, 10, 20, 40, millisecond);
        //        Assert.AreEqual(expected, result);
        //    }
        //}

        private IContainer InitializeContainer(string cultureName) {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<WorkContext>(new StubWorkContext(cultureName));
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<CultureDateTimeFormatProvider>().As<IDateTimeFormatProvider>();
            builder.RegisterType<DefaultDateFormatter>().As<IDateFormatter>();
            builder.RegisterInstance(new Mock<ICalendarManager>().Object);
            return builder.Build();
        }

        private class StubWorkContext : WorkContext {

            private string _cultureName;

            public StubWorkContext(string cultureName) {
                _cultureName = cultureName;
            }

            public override T Resolve<T>() {
                throw new NotImplementedException();
            }

            public override bool TryResolve<T>(out T service) {
                throw new NotImplementedException();
            }

            public override T GetState<T>(string name) {
                if (name == "CurrentCulture") return (T)((object)_cultureName);
                if (name == "CurrentCalendar") return (T)default(object);
                throw new NotImplementedException(String.Format("Property '{0}' is not implemented.", name));
            }

            public override void SetState<T>(string name, T value) {
                throw new NotImplementedException();
            }
        }

        private class StubWorkContextAccessor : IWorkContextAccessor {

            private WorkContext _workContext;

            public StubWorkContextAccessor(WorkContext workContext) {
                _workContext = workContext;
            }

            public WorkContext GetContext(System.Web.HttpContextBase httpContext) {
                throw new NotImplementedException();
            }

            public IWorkContextScope CreateWorkContextScope(System.Web.HttpContextBase httpContext) {
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
}
