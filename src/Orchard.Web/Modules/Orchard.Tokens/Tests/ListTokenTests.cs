using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Tokens.Implementation;
using Orchard.Tokens.Providers;

namespace Orchard.Tokens.Tests {
    [TestFixture]
    public class ListTokenTests {
        private IContainer _container;
        private ITokenizer _tokenizer;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<Tokenizer>().As<ITokenizer>();
            builder.RegisterType<TokenManager>().As<ITokenManager>();
            builder.RegisterType<ListTokens>().As<ITokenProvider>();
            builder.RegisterType<TestTokenProvider>().As<ITokenProvider>();
            //builder.RegisterType<Work<Tokenizer>>().As<Work<ITokenizer>>();
            _container = builder.Build();
            _tokenizer = _container.Resolve<ITokenizer>();
        }

        [Test]
        public void TestListJoinTokens() {
            Assert.That(_tokenizer.Replace("{List.Join:{Content.Id}}", new {List = new List<IContent> {new TestUser {Id = 5}, new TestUser {Id = 10}}}), Is.EqualTo("510"));
        }

        [Test]
        public void TestListJoinWithSeparatorTokens() {
            Assert.That(_tokenizer.Replace("{List.Join:UserId is: {Content.Id},, }", new { List = new List<IContent> { new TestUser { Id = 5 }, new TestUser { Id = 10 } } }), Is.EqualTo("UserId is: 5, UserId is: 10"));
        }

        [Test]
        public void TestListSumTokens() {
            Assert.That(_tokenizer.Replace("{List.Sum:{Content.Id}}", new {List = new List<IContent> {new TestUser {Id = 5}, new TestUser {Id = 10}}}), Is.EqualTo("15"));
        }

        [Test]
        public void TestListFirstTokens() {
            Assert.That(_tokenizer.Replace("{List.First:{Content.Id}}", new {List = new List<IContent> {new TestUser {Id = 5}, new TestUser {Id = 10}}}), Is.EqualTo("5"));
        }

        [Test]
        public void TestListLastTokens() {
            Assert.That(_tokenizer.Replace("{List.Last:{Content.Id}}", new {List = new List<IContent> {new TestUser {Id = 5}, new TestUser {Id = 10}}}), Is.EqualTo("10"));
        }

        [Test]
        public void TestListCountTokens() {
            Assert.That(_tokenizer.Replace("{List.Count}", new {List = new List<IContent> {new TestUser(), new TestUser()}}), Is.EqualTo("2"));
		}

		[Test]
		public void TestListElementAtTokens() {
			Assert.That(_tokenizer.Replace("{List.ElementAt:1,UserId is: {Content.Id}}", new { List = new List<IContent> { new TestUser { Id = 5 }, new TestUser { Id = 10 } } }), Is.EqualTo("UserId is: 10"));
		}

		[Test]
        public void TestListTokensAdhereToEncodings() {
            Assert.That(_tokenizer.Replace(
                "{List.Join:<strong>{Content.Id}<strong>}",
                new {List = new List<IContent> {new TestUser {Id = 5}, new TestUser {Id = 10}}}, 
                new ReplaceOptions {Encoding = ReplaceOptions.HtmlEncode}),
                Is.EqualTo("&lt;strong&gt;5&lt;strong&gt;&lt;strong&gt;10&lt;strong&gt;"));
            Assert.That(_tokenizer.Replace(
                "{List.Join:<strong>{Content.Id}<strong>}",
                new {List = new List<IContent> {new TestUser {Id = 5}, new TestUser {Id = 10}}}, 
                new ReplaceOptions {Encoding = ReplaceOptions.NoEncode}),
                Is.EqualTo("<strong>5<strong><strong>10<strong>"));
        }

    }
}