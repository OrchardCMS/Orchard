using System;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Indexing;
using Orchard.Core.Indexing.Lucene;

namespace Orchard.Tests.Indexing {
    public class DefaultSearchBuilderTests {
        private IContainer _container;
        private IIndexProvider _provider;
        private IAppDataFolder _appDataFolder;
        private ShellSettings _shellSettings;
        private readonly string _basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        
        [TestFixtureTearDown]
        public void Clean() {
            Directory.Delete(_basePath, true);
        }

        [SetUp]
        public void Setup() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
            Directory.CreateDirectory(_basePath);


            _appDataFolder = new AppDataFolder();
            _appDataFolder.SetBasePath(_basePath);

            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultIndexProvider>().As<IIndexProvider>();
            builder.RegisterInstance(_appDataFolder).As<IAppDataFolder>();

            // setting up a ShellSettings instance
            _shellSettings = new ShellSettings { Name = "My Site" };
            builder.RegisterInstance(_shellSettings).As<ShellSettings>();

            _container = builder.Build();
            _provider = _container.Resolve<IIndexProvider>();
        }

        private ISearchBuilder _searchBuilder { get { return _provider.CreateSearchBuilder("default"); } }

        [Test]
        public void SearchTermsShouldBeFoundInMultipleFields() {
            _provider.CreateIndex("default");
            _provider.Store("default", 
                _provider.New(42)
                    .Add("title", "title1 title2 title3")
                    .Add("date", new DateTime(2010, 05, 28, 14, 13, 56, 123))
                );

            Assert.IsNotNull(_provider.CreateSearchBuilder("default").Get(42));

            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title1").Search().FirstOrDefault());
            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title2").Search().FirstOrDefault());
            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title3").Search().FirstOrDefault());
            Assert.IsNull(_provider.CreateSearchBuilder("default").WithField("title", "title4").Search().FirstOrDefault());
            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title").Search().FirstOrDefault());

        }

        [Test]
        public void ShouldSearchById() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1));
            _provider.Store("default", _provider.New(2));
            _provider.Store("default", _provider.New(3));


            Assert.That(_searchBuilder.Get(1).Id, Is.EqualTo(1));
            Assert.That(_searchBuilder.Get(2).Id, Is.EqualTo(2));
            Assert.That(_searchBuilder.Get(3).Id, Is.EqualTo(3));
        }

        [Test]
        public void ShouldSearchWithField() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("title", "cat"));
            _provider.Store("default", _provider.New(2).Add("title", "dog"));
            _provider.Store("default", _provider.New(3).Add("title", "cat"));


            Assert.That(_searchBuilder.WithField("title", "cat").Search().Count(), Is.EqualTo(2));
            Assert.That(_searchBuilder.WithField("title", "cat").Search().Any(hit => new[] { 1, 3 }.Contains(hit.Id)), Is.True);

        }

        [Test]
        public void ShouldCountResultsOnly() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("title", "cat"));
            _provider.Store("default", _provider.New(2).Add("title", "dog"));
            _provider.Store("default", _provider.New(3).Add("title", "cat"));

            Assert.That(_searchBuilder.WithField("title", "dog").Count(), Is.EqualTo(1));
            Assert.That(_searchBuilder.WithField("title", "cat").Count(), Is.EqualTo(2));
            Assert.That(_searchBuilder.WithField("title", "c").Count(), Is.EqualTo(2));

        }

        [Test]
        public void ShouldFilterByDate() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("date", new DateTime(2010, 05, 28, 12, 30, 15)));
            _provider.Store("default", _provider.New(2).Add("date", new DateTime(2010, 05, 28, 12, 30, 30)));
            _provider.Store("default", _provider.New(3).Add("date", new DateTime(2010, 05, 28, 12, 30, 45)));

            Assert.That(_searchBuilder.After("date", new DateTime(2010, 05, 28, 12, 30, 15)).Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.Before("date", new DateTime(2010, 05, 28, 12, 30, 45)).Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.After("date", new DateTime(2010, 05, 28, 12, 30, 15)).Before("date", new DateTime(2010, 05, 28, 12, 30, 45)).Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.After("date", new DateTime(2010, 05, 28, 12, 30, 16)).Before("date", new DateTime(2010, 05, 28, 12, 30, 44)).Count(), Is.EqualTo(1));
            Assert.That(_searchBuilder.After("date", new DateTime(2010, 05, 28, 12, 30, 46)).Count(), Is.EqualTo(0));
            Assert.That(_searchBuilder.Before("date", new DateTime(2010, 05, 28, 12, 30, 1)).Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldSliceResults() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1));
            _provider.Store("default", _provider.New(22));
            _provider.Store("default", _provider.New(333));
            _provider.Store("default", _provider.New(4444));
            _provider.Store("default", _provider.New(55555));

            
            Assert.That(_searchBuilder.Count(), Is.EqualTo(5));
            Assert.That(_searchBuilder.Slice(0, 3).Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.Slice(1, 3).Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.Slice(3, 3).Count(), Is.EqualTo(2));

            // Count() and Search() should return the same results
            Assert.That(_searchBuilder.Search().Count(), Is.EqualTo(5));
            Assert.That(_searchBuilder.Slice(0, 3).Search().Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.Slice(1, 3).Search().Count(), Is.EqualTo(3));
            Assert.That(_searchBuilder.Slice(3, 3).Search().Count(), Is.EqualTo(2));
        }

        [Test]
        public void ShouldSortByRelevance() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "michaelson is in the kitchen"));
            _provider.Store("default", _provider.New(2).Add("body", "michael as a cousin named michael"));
            _provider.Store("default", _provider.New(3).Add("body", "speak inside the mic"));
            _provider.Store("default", _provider.New(4).Add("body", "a dog is pursuing a cat"));
            _provider.Store("default", _provider.New(5).Add("body", "the elephant can't catch up the dog"));

            var michael = _searchBuilder.WithField("body", "mic").Search().ToList();
            Assert.That(michael.Count(), Is.EqualTo(3));
            Assert.That(michael[0].Score >= michael[1].Score, Is.True);

            // Sorting on score is always descending
            michael = _searchBuilder.WithField("body", "mic").Ascending().Search().ToList();
            Assert.That(michael.Count(), Is.EqualTo(3));
            Assert.That(michael[0].Score >= michael[1].Score, Is.True);
        }

        [Test]
        public void ShouldSortByDate() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("date", new DateTime(2010, 05, 28, 12, 30, 15)));
            _provider.Store("default", _provider.New(2).Add("date", new DateTime(2010, 05, 28, 12, 30, 30)));
            _provider.Store("default", _provider.New(3).Add("date", new DateTime(2010, 05, 28, 12, 30, 45)));

            var date = _searchBuilder.SortBy("date").Search().ToList();
            Assert.That(date.Count(), Is.EqualTo(3));
            Assert.That(date[0].GetDateTime("date") > date[1].GetDateTime("date"), Is.True);
            Assert.That(date[1].GetDateTime("date") > date[2].GetDateTime("date"), Is.True);

            date = _searchBuilder.SortBy("date").Ascending().Search().ToList();
            Assert.That(date.Count(), Is.EqualTo(3));
            Assert.That(date[0].GetDateTime("date") < date[1].GetDateTime("date"), Is.True);
            Assert.That(date[1].GetDateTime("date") < date[2].GetDateTime("date"), Is.True);
        }
    }
}
