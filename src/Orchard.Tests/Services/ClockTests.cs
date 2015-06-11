using System;
using System.Threading;
using NUnit.Framework;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Services {

    [TestFixture]
    public class ClockTests {
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

