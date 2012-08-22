using System;
using Autofac;
using NUnit.Framework;
using Orchard.Tokens.Implementation;

namespace Orchard.Tokens.Tests {
    [TestFixture]
    public class TokenizerTests {
        private IContainer _container;
        private ITokenizer _tokenizer;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<Tokenizer>().As<ITokenizer>();
            builder.RegisterType<TokenManager>().As<ITokenManager>();
            builder.RegisterType<TestTokenProvider>().As<ITokenProvider>();
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
        }

        [Test]
        public void TestGlobalTokens() {
            Assert.That(_tokenizer.Replace("{Site.Global1}", null), Is.EqualTo("[global1]"));
            Assert.That(_tokenizer.Replace("{Site.Global2}", null), Is.EqualTo("[global2]"));
            Assert.That(_tokenizer.Replace("{Site.Global1}{Site.Global2}{Site.Global1}{Site.Global2}", null), Is.EqualTo("[global1][global2][global1][global2]"));
        }

        [Test]
        public void TestContextTokens() {
            Assert.That(_tokenizer.Replace("{User.Name}", null), Is.EqualTo("CurrentUser"));
            Assert.That(_tokenizer.Replace("{User.Name}", new { User = new TestUser { UserName = "LocalUser" } }), Is.EqualTo("LocalUser"));
        }

        [Test]
        public void TestChainedTokens() {
            Assert.That(_tokenizer.Replace("{Site.CurrentUser.Name}", null), Is.EqualTo("CurrentUser"));
            Assert.That(_tokenizer.Replace("{Site.CurrentUser.Name}", new { User = new TestUser { UserName = "ShouldStillUseParentValue" } }), Is.EqualTo("CurrentUser"));
            Assert.That(_tokenizer.Replace("{Site.CurrentUser.Birthdate}", null), Is.EqualTo("Nov 15"));
            Assert.That(_tokenizer.Replace("{Site.CurrentUser.Birthdate.yyyy}", null), Is.EqualTo("1978"));
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
            Assert.That(_tokenizer.Replace("{site.Global1}", null), Is.EqualTo(""));
            Assert.That(_tokenizer.Replace("{Site.global1}", null), Is.EqualTo(""));
        }

        [Test]
        public void TestTokenEscapeSequences() {
            Assert.That(_tokenizer.Replace("{{escaped}} {Site.Global1} }}{{ {{{{ }}}}", null), Is.EqualTo("{escaped} [global1] }{ {{ }}"));
            Assert.That(_tokenizer.Replace("{Date.Now.{{yyyy}}}", null), Is.EqualTo(DateTime.UtcNow.ToString("{yyyy}")));
        }

        [Test]
        public void TestHtmlEncodedByDefault() {
            Assert.That(_tokenizer.Replace("{Date.Now.<>}", null), Is.EqualTo("&lt;&gt;"));
        }

        [Test]
        public void TestNoEncode() {
            Assert.That(_tokenizer.Replace("{Date.Now.<>}", null, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode }), Is.EqualTo("<>"));
        }

        [Test]
        public void TestPredicate() {
            Assert.That(_tokenizer.Replace("{Site.Global1}{Site.Global2}", null, new ReplaceOptions { Predicate = token => token == "Site.Global2" }), Is.EqualTo("{Site.Global1}[global2]"));
        }
    }
}
