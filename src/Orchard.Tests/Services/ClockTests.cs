using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Services {

    [TestFixture]
    public class ClockTests {
        [Test, Ignore("At the moment the default clock is using DateTime.Now until a user-time-zone corrected display is in effect.")]
        public void DefaultClockShouldComeFromSystemUtc() {
            IClock clock = new Clock();
            var before = DateTime.UtcNow;
            Thread.Sleep(2);
            var mark = clock.UtcNow;
            Thread.Sleep(2);
            var after = DateTime.UtcNow;

            Assert.That(mark.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(mark, Is.InRange(before, after));
        }

        [Test]
        public void StubClockShouldComeFromSystemUtcAndDoesNotComeFromSystemTime() {
            var clock = new StubClock();
            var before = DateTime.UtcNow;
            Thread.Sleep(2);
            var mark = clock.UtcNow;
            Thread.Sleep(2);
            var after = DateTime.UtcNow;

            Assert.That(mark.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(mark, Is.Not.InRange(before, after));
        }

        [Test]
        public void StubClockCanBeManuallyAdvanced() {
            var clock = new StubClock();
            var before = clock.UtcNow;
            clock.Advance(TimeSpan.FromMilliseconds(2));
            var mark = clock.UtcNow;
            clock.Advance(TimeSpan.FromMilliseconds(2));
            var after = clock.UtcNow;

            Assert.That(mark.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(mark, Is.InRange(before, after));
            Assert.That(after.Subtract(before), Is.EqualTo(TimeSpan.FromMilliseconds(4)));
        }
    }
}

