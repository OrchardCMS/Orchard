using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Orchard.Pages.Services.Templates;

namespace Orchard.Tests.Packages.Pages.Services.Templates {
    [TestFixture]
    public class CommentExtractorTests {
        [Test]
        public void ExtractorShouldReturnEmptyWhenNoComment() {
            var reader = new StringReader("  \r\n   ");
            var extractor = new CommentExtractor();
            IList<string> result = extractor.Process(reader);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExtractorShouldReturnOneComment() {
            var reader = new StringReader("<%@Page %><%--n--%>");

            var extractor = new CommentExtractor();
            IList<string> result = extractor.Process(reader);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo("n"));
        }

        [Test]
        public void ExtractorShouldReturnManyComment() {
            var reader = new StringReader(@"
<%@Page %>
<%-- n1 --%>
<p></p>
<%-- n2 --%>
text
<%-- n3 --%>
<%--
 n4 
--%>
");

            var extractor = new CommentExtractor();
            IList<string> result = extractor.Process(reader);

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result[0], Is.EqualTo(" n1 "));
            Assert.That(result[1], Is.EqualTo(" n2 "));
            Assert.That(result[2], Is.EqualTo(" n3 "));
            Assert.That(result[3], Is.EqualTo("\r\n n4 \r\n"));
        }

        [Test]
        public void ExtractorShouldReturnFirstComment() {
            var reader = new StringReader(@"
<%@Page %>
<%-- n1 --%>
<p></p>
<%-- n2 --%>
text
<%-- n3 --%>
<%-- n4 --%>
");

            var extractor = new CommentExtractor();
            string result = extractor.FirstComment(reader).ReadToEnd();

            Assert.That(result, Is.EqualTo(" n1 "));
        }

        [Test]
        public void ExtractorShouldReturnEmptyFirstComment() {
            var reader = new StringReader(@"
<%@Page %>
<%-- n1 
");

            var extractor = new CommentExtractor();
            string result = extractor.FirstComment(reader).ReadToEnd();

            Assert.That(result, Is.EqualTo(""));
        }
    }
}