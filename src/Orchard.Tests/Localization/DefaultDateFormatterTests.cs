using System;
using System.Globalization;
using NUnit.Framework;
using Orchard.Framework.Localization.Models;
using Orchard.Framework.Localization.Services;

namespace Orchard.Framework.Tests.Localization {

    [TestFixture]
    public class DefaultDateFormatterTests {

        [Test]
        [Description("Correct Swedish date is parsed correctly.")]
        public void ParseTest01() {
            IDateFormatter target = new DefaultDateFormatter();
            var cultureInfo = CultureInfo.GetCultureInfo("sv-SE");
            var dateTimeString = "2014-05-31 10:00:00";

            var result = target.ParseDateTime(dateTimeString, cultureInfo);

            var expected = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [Description("Correct US English date is parsed correctly.")]
        public void ParseTest02() {
            IDateFormatter target = new DefaultDateFormatter();
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");

            var dateString = "5/31/2014 10:00:00 AM";
            var result = target.ParseDateTime(dateString, cultureInfo);

            var expected = new DateTimeParts(2014, 5, 31, 10, 0, 0, 0);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [Description("Incorrect US English date yields a FormatException.")]
        [ExpectedException(typeof(FormatException))]
        public void ParseTest03() {
            IDateFormatter target = new DefaultDateFormatter();
            var cultureInfo = CultureInfo.GetCultureInfo("en-US");
            var dateString = "blablabla";

            target.ParseDateTime(dateString, cultureInfo);
        }

        [Test]
        [Description("Loop through all cultures. Test Parse method by all possible DateTimeFormats.")]
        public void ParseTest04() {
            IDateFormatter target = new DefaultDateFormatter();
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (CultureInfo cultureInfo in cultures) {
                // Due to a bug in .NET 4.5 in combination with updated Upper Sorbian culture in Windows 8.
                if (System.Environment.OSVersion.Version.ToString().CompareTo("6.2.0.0") >= 0 && cultureInfo.Name.StartsWith("hsb")) {
                    continue;
                }
                DateTime dateTime = new DateTime(2014, 12, 31, 10, 20, 40, 567);
                cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
                var dateString = dateTime.ToString("G", cultureInfo);
                var result = target.ParseDateTime(dateString, cultureInfo);
                var millisecond = DateTime.Parse(dateString, cultureInfo.DateTimeFormat).Millisecond;
                var expected = new DateTimeParts(2014, 12, 31, 10, 20, 40, millisecond);
                Assert.AreEqual(expected, result);
            }
        }
    }
}
