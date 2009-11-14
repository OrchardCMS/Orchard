using NUnit.Framework;
using Orchard.UI.Notify;

namespace Orchard.Tests.UI.Notify {
    [TestFixture]
    public class NotifierTests {

        [Test]
        public void MessageServiceCanAccumulateWarningsAndErrorsToReturn() {
            INotifier notifier = new Notifier();
            notifier.Warning("Hello world");
            notifier.Information("More Info");
            notifier.Error("Boom");

            Assert.That(notifier.List(), Has.Count.EqualTo(3));
            foreach (var notifyEntries in notifier.List()) {
                Assert.That(new[] {notifyEntries.Message.ToString()}, Is.SubsetOf(new[]
                {
                    "Hello world", "More Info", "Boom"
                }));
            }
        }
    }
}