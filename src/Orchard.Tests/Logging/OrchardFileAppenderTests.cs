using System;
using log4net.Util;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Orchard.Logging;

namespace Orchard.Tests.Logging {
    [TestFixture]
    public class OrchardFileAppenderTests {
        [Test]
        public void AddSuffixTest() {
            const string filename = "Orchard-debug";
            const string filenameAlternative1 = "Orchard-debug-1";
            const string filenameAlternative2 = "Orchard-debug-2";

            string filenameUsed = string.Empty;

            // Set logging to quiet mode
            LogLog.QuietMode = true;

            Mock<StubOrchardFileAppender> firstOrchardFileAppenderMock = new Mock<StubOrchardFileAppender>();
            StubOrchardFileAppender firstOrchardFileAppender = firstOrchardFileAppenderMock.Object;

            // Regular filename should be used if nothing is locked
            firstOrchardFileAppenderMock.Protected().Setup("BaseOpenFile", ItExpr.Is<string>(file => file.Equals(filename)), ItExpr.IsAny<bool>()).Callback<string, bool>(
                    (file, append) => filenameUsed = file);

            firstOrchardFileAppender.OpenFileStub(filename, true);

            Assert.That(filenameUsed, Is.EqualTo(filename));

            // Alternative 1 should be used if regular filename is locked

            firstOrchardFileAppenderMock.Protected().Setup("BaseOpenFile", ItExpr.Is<string>(file => file.Equals(filename)), ItExpr.IsAny<bool>()).Throws(new Exception());
            firstOrchardFileAppenderMock.Protected().Setup("BaseOpenFile", ItExpr.Is<string>(file => file.Equals(filenameAlternative1)), ItExpr.IsAny<bool>()).Callback<string, bool>(
                    (file, append) => filenameUsed = file);

            firstOrchardFileAppender.OpenFileStub(filename, true);

            Assert.That(filenameUsed, Is.EqualTo(filenameAlternative1));

            // make alternative 1 also throw exception to make sure alternative 2 is used.
            firstOrchardFileAppenderMock.Protected().Setup("BaseOpenFile", ItExpr.Is<string>(file => file.Equals(filenameAlternative1)), ItExpr.IsAny<bool>()).Throws(new Exception());
            firstOrchardFileAppenderMock.Protected().Setup("BaseOpenFile", ItExpr.Is<string>(file => file.Equals(filenameAlternative2)), ItExpr.IsAny<bool>()).Callback<string, bool>(
                    (file, append) => filenameUsed = file);

            firstOrchardFileAppender.OpenFileStub(filename, true);

            Assert.That(filenameUsed, Is.EqualTo(filenameAlternative2));
        }

        public class StubOrchardFileAppender : OrchardFileAppender {
            public void OpenFileStub(string fileName, bool append) {
                base.OpenFile(fileName, append);
            }
        }
    }
}
