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
            Assert.That(notifier.List(), Has.Some.Property("Message").EqualTo("Hello world"));
            Assert.That(notifier.List(), Has.Some.Property("Message").EqualTo("More Info"));
            Assert.That(notifier.List(), Has.Some.Property("Message").EqualTo("Boom"));
        }
    }
}