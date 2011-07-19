using System;
using System.Linq;
using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Time;
using Orchard.Tokens;

namespace Orchard.Tests.Tokens {
    [TestFixture]
    public class TokenizerTests {
        private IContainer _container;
        private ITokenizer _tokenizer;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultTokenizer>().As<ITokenizer>();
            builder.RegisterType<DefaultTokenManager>().As<ITokenManager>();
            builder.RegisterType<TestTokenProvider>().As<ITokenProvider>();
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
        }

        [Test]
        public void TestGlobalTokens() {
            Assert.That(_tokenizer.Replace("{Site.Global1}", null), Is.EqualTo("[global1]"));
            Assert.That(_tokenizer.Replace("{Site.Global2}", null), Is.EqualTo("[global2]"));
        }

        [Test]
        public void TestContextTokens() {
            var testUser = new TestUser { UserName = "OrchardUser" };
            Assert.That(_tokenizer.Replace("{User.FullName}", new { User = testUser }), Is.EqualTo("[OrchardUser]"));
            Assert.That(_tokenizer.Replace("{Site.CurrentUser.FullName}", null), Is.EqualTo("[CurrentUser]"));
            // context prefers the value of a parent token
            Assert.That(_tokenizer.Replace("{Site.CurrentUser.FullName}", new { User = new TestUser { UserName = "UserFromContext" } }), Is.EqualTo("[CurrentUser]"));
        }

        [Test]
        public void TestMissingTokens() {
            Assert.That(_tokenizer.Replace("[{Site.NotAToken}]", null), Is.EqualTo("[]"));
            Assert.That(_tokenizer.Replace("[{NotATokenType.Foo}]", null), Is.EqualTo("[]"));
            Assert.That(_tokenizer.Replace("[{Site.CurrentUser.NotASubToken}]", null), Is.EqualTo("[]"));
            Assert.That(_tokenizer.Replace("[{Site}]", null), Is.EqualTo("[]"));
            Assert.That(_tokenizer.Replace("[{NotATokenType}]", null), Is.EqualTo("[]"));
        }

        [Test]
        public void TestTokenCaseSensitivity() {
            Assert.That(_tokenizer.Replace("{Site.Global1}", null), Is.EqualTo("[global1]"));
            Assert.That(_tokenizer.Replace("{site.global1}", null), Is.EqualTo("[global1]"));
        }

        [Test]
        public void TestTokenEscapeSequences() {
            Assert.That(_tokenizer.Replace("{{escaped}} {Site.Global1} }}{{ {{{{ }}}}", null), Is.EqualTo("{escaped} [global1] }{ {{ }}"));
            Assert.That(_tokenizer.Replace("{Site.Date:{{YYYY}}}", null), Is.EqualTo(DateTime.UtcNow.ToString("{YYYY}")));
        }

        [Test]
        public void TestTokenWhitespace() {
            Assert.That(_tokenizer.Replace("{ Site . Global1 }", null), Is.EqualTo("[global1]"));
            Assert.That(_tokenizer.Replace("{ Site . Date : YYYY }", null), Is.EqualTo(DateTime.UtcNow.ToString(" YYYY ")));
        }

        [Test]
        public void TestTokensAsProperties() {
            Assert.That(_tokenizer.Replace("[{Site.Date.Year}]", null), Is.EqualTo("[" + DateTime.UtcNow.Year + "]"));
            Assert.That(_tokenizer.Replace("[{Site.Date.Year:d5}]", null), Is.EqualTo("[" + DateTime.UtcNow.Year.ToString("d5") + "]"));
            Assert.That(_tokenizer.Replace("[{User.FirstName.Length}]", new { User = new TestUser { FirstName = "Joe" } }), Is.EqualTo("[3]"));
        }

        [Test]
        public void TestParseTokens() {
            //         01234567890123456789012345678901234567890123
            var str = "01234{Site.Global1}{{}}{Site.Global2:format}";
            var tokenContexts = _tokenizer.ParseTokens(str, null).ToList();
            Assert.That(tokenContexts.Count, Is.EqualTo(4));
            VerifyToken(tokenContexts[0], 5, "{Site.Global1}".Length, "[global1]");
            VerifyToken(tokenContexts[1], 19, "{{".Length, "{");
            VerifyToken(tokenContexts[2], 21, "}}".Length, "}");
            VerifyToken(tokenContexts[3], 23, "{Site.Global2:format}".Length, "[global2]");
            tokenContexts[0].Replacement = "[replace1]";
            tokenContexts[3].Replacement = "[replace2]";
            str = _tokenizer.Replace(tokenContexts, str);
            Assert.That(str, Is.EqualTo("01234[replace1]{}[replace2]"));
        }

        public void VerifyToken(TokenContext tc, int offset, int length, string replacement) {
            Assert.That(tc.Offset, Is.EqualTo(offset), "Offset");
            Assert.That(tc.Length, Is.EqualTo(length), "Length");
            Assert.That(tc.Replacement, Is.EqualTo(replacement), "Replacement");
        }

    }

    public class TestUser : IUser {
        public string UserName { get; set;}
        public string Email { get; set; }
        public ContentItem ContentItem { get; set; }
        public int Id { get; set; }

        // no token provided for FirstName
        public string FirstName { get; set; }
    }

    public class TestTokenProvider : ITokenProvider {
        public void BuildTokens(TokenBuilder builder) {
            builder.Describe("Site", "Global1", ctx => "[global1]");
            builder.Describe("site", "Global2", ctx => "[global2]");

            builder.Describe<IUser>("Site", "CurrentUser").WithValue("User", ctx => new TestUser { UserName = "CurrentUser" });
            builder.Describe<IUser>("User", "FullName", user => "[" + user.UserName + "]");

            builder.Describe("Site", "Date", ctx => DateTime.UtcNow);

        }
    }
}

