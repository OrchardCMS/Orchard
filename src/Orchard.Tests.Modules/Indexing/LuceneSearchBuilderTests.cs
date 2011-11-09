using System;
using System.IO;
using System.Linq;
using Autofac;
using Lucene.Services;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Indexing;
using Orchard.Tests.FileSystems.AppData;

namespace Orchard.Tests.Modules.Indexing {
    public class LuceneSearchBuilderTests {
        private IContainer _container;
        private IIndexProvider _provider;
        private IAppDataFolder _appDataFolder;
        private ShellSettings _shellSettings;
        private readonly string _basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        
        [TestFixtureTearDown]
        public void Clean() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
        }

        [SetUp]
        public void Setup() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
            Directory.CreateDirectory(_basePath);

            _appDataFolder = AppDataFolderTests.CreateAppDataFolder(_basePath);

            var builder = new ContainerBuilder();
            builder.RegisterType<LuceneIndexProvider>().As<IIndexProvider>();
            builder.RegisterInstance(_appDataFolder).As<IAppDataFolder>();

            // setting up a ShellSettings instance
            _shellSettings = new ShellSettings { Name = "My Site" };
            builder.RegisterInstance(_shellSettings).As<ShellSettings>();

            _container = builder.Build();
            _provider = _container.Resolve<IIndexProvider>();
        }

        private ISearchBuilder SearchBuilder { get { return _provider.CreateSearchBuilder("default"); } }

        [Test]
        public void SearchTermsShouldBeFoundInMultipleFields() {
            _provider.CreateIndex("default");
            _provider.Store("default", 
                _provider.New(42)
                    .Add("title", "title1 title2 title3").Analyze()
                    .Add("date", new DateTime(2010, 05, 28, 14, 13, 56, 123))
                );

            Assert.IsNotNull(_provider.CreateSearchBuilder("default").Get(42));

            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title1").Search().FirstOrDefault());
            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title2").Search().FirstOrDefault());
            Assert.IsNotNull(_provider.CreateSearchBuilder("default").WithField("title", "title3").Search().FirstOrDefault());
            Assert.IsNull(_provider.CreateSearchBuilder("default").WithField("title", "title4").Search().FirstOrDefault());

        }

        [Test]
        public void ShouldSearchById() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1));
            _provider.Store("default", _provider.New(2));
            _provider.Store("default", _provider.New(3));


            Assert.That(SearchBuilder.Get(1).ContentItemId, Is.EqualTo(1));
            Assert.That(SearchBuilder.Get(2).ContentItemId, Is.EqualTo(2));
            Assert.That(SearchBuilder.Get(3).ContentItemId, Is.EqualTo(3));
        }

        [Test]
        public void ShouldSearchWithField() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("title", "cat"));
            _provider.Store("default", _provider.New(2).Add("title", "dog"));
            _provider.Store("default", _provider.New(3).Add("title", "cat"));


            Assert.That(SearchBuilder.WithField("title", "cat").Search().Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("title", "cat").Search().Any(hit => new[] { 1, 3 }.Contains(hit.ContentItemId)), Is.True);
        }

        [Test]
        public void ShouldSearchByBooleanWhateverIndexingScheme() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("foo", true));
            _provider.Store("default", _provider.New(2).Add("foo", true).Store());
            _provider.Store("default", _provider.New(3).Add("foo", true).Analyze());
            _provider.Store("default", _provider.New(4).Add("foo", true).Store().Analyze());
            _provider.Store("default", _provider.New(5).Add("foo", false));
            _provider.Store("default", _provider.New(6).Add("foo", false).Store());
            _provider.Store("default", _provider.New(7).Add("foo", false).Analyze());
            _provider.Store("default", _provider.New(8).Add("foo", false).Store().Analyze());

            Assert.That(SearchBuilder.WithField("foo", false).Search().Count(), Is.EqualTo(4));
            Assert.That(SearchBuilder.WithField("foo", true).Search().Count(), Is.EqualTo(4));
        }

        [Test]
        public void ShouldSearchByStringWhateverIndexingScheme() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("foo", "abc"));
            _provider.Store("default", _provider.New(2).Add("foo", "abc").Store());
            _provider.Store("default", _provider.New(3).Add("foo", "abc").Analyze());
            _provider.Store("default", _provider.New(4).Add("foo", "abc").Store().Analyze());
            _provider.Store("default", _provider.New(5).Add("foo", "def"));
            _provider.Store("default", _provider.New(6).Add("foo", "def").Store());
            _provider.Store("default", _provider.New(7).Add("foo", "def").Analyze());
            _provider.Store("default", _provider.New(8).Add("foo", "def").Store().Analyze());

            Assert.That(SearchBuilder.WithField("foo", "abc").Search().Count(), Is.EqualTo(4));
            Assert.That(SearchBuilder.WithField("foo", "def").Search().Count(), Is.EqualTo(4));
        }

        [Test]
        public void ShouldSearchByIntegerWhateverIndexingScheme() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("foo", 1));
            _provider.Store("default", _provider.New(2).Add("foo", 1).Store());
            _provider.Store("default", _provider.New(3).Add("foo", 1).Analyze());
            _provider.Store("default", _provider.New(4).Add("foo", 1).Store().Analyze());
            _provider.Store("default", _provider.New(5).Add("foo", 2));
            _provider.Store("default", _provider.New(6).Add("foo", 2).Store());
            _provider.Store("default", _provider.New(7).Add("foo", 2).Analyze());
            _provider.Store("default", _provider.New(8).Add("foo", 2).Store().Analyze());

            Assert.That(SearchBuilder.WithField("foo", 1).Search().Count(), Is.EqualTo(4));
            Assert.That(SearchBuilder.WithField("foo", 2).Search().Count(), Is.EqualTo(4));
        }

        [Test]
        public void ShouldSearchByFloatWhateverIndexingScheme() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("foo", 1.1));
            _provider.Store("default", _provider.New(2).Add("foo", 1.1).Store());
            _provider.Store("default", _provider.New(3).Add("foo", 1.1).Analyze());
            _provider.Store("default", _provider.New(4).Add("foo", 1.1).Store().Analyze());
            _provider.Store("default", _provider.New(5).Add("foo", 2.1));
            _provider.Store("default", _provider.New(6).Add("foo", 2.1).Store());
            _provider.Store("default", _provider.New(7).Add("foo", 2.1).Analyze());
            _provider.Store("default", _provider.New(8).Add("foo", 2.1).Store().Analyze());

            Assert.That(SearchBuilder.WithField("foo", 1.1).Search().Count(), Is.EqualTo(4));
            Assert.That(SearchBuilder.WithField("foo", 2.1).Search().Count(), Is.EqualTo(4));
        }

        [Test]
        public void ShouldSearchByDateTimeWhateverIndexingScheme() {
            var date1 = DateTime.Today;
            var date2 = DateTime.Today.AddDays(1);
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("foo", date1));
            _provider.Store("default", _provider.New(2).Add("foo", date1).Store());
            _provider.Store("default", _provider.New(3).Add("foo", date1).Analyze());
            _provider.Store("default", _provider.New(4).Add("foo", date1).Store().Analyze());
            _provider.Store("default", _provider.New(5).Add("foo", date2));
            _provider.Store("default", _provider.New(6).Add("foo", date2).Store());
            _provider.Store("default", _provider.New(7).Add("foo", date2).Analyze());
            _provider.Store("default", _provider.New(8).Add("foo", date2).Store().Analyze());

            Assert.That(SearchBuilder.WithField("foo", date1).Search().Count(), Is.EqualTo(4));
            Assert.That(SearchBuilder.WithField("foo", date2).Search().Count(), Is.EqualTo(4));
        }


        [Test]
        public void ShouldCountResultsOnly() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("title", "cat"));
            _provider.Store("default", _provider.New(2).Add("title", "dog"));
            _provider.Store("default", _provider.New(3).Add("title", "cat"));

            Assert.That(SearchBuilder.WithField("title", "dog").Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("title", "cat").Count(), Is.EqualTo(2));
        }

        [Test]
        public void ShouldFilterByDate() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("date", new DateTime(2010, 05, 28, 12, 30, 15)));
            _provider.Store("default", _provider.New(2).Add("date", new DateTime(2010, 05, 28, 12, 30, 30)));
            _provider.Store("default", _provider.New(3).Add("date", new DateTime(2010, 05, 28, 12, 30, 45)));

            Assert.That(SearchBuilder.WithinRange("date", new DateTime(2010, 05, 28, 12, 30, 15), DateTime.MaxValue).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.WithinRange("date", DateTime.MinValue, new DateTime(2010, 05, 28, 12, 30, 45)).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.WithinRange("date", new DateTime(2010, 05, 28, 12, 30, 15), new DateTime(2010, 05, 28, 12, 30, 45)).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.WithinRange("date", new DateTime(2010, 05, 28, 12, 30, 16), new DateTime(2010, 05, 28, 12, 30, 44)).Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithinRange("date", new DateTime(2010, 05, 28, 12, 30, 46), DateTime.MaxValue).Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithinRange("date", DateTime.MinValue, new DateTime(2010, 05, 28, 12, 30, 1)).Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldSliceResults() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1));
            _provider.Store("default", _provider.New(22));
            _provider.Store("default", _provider.New(333));
            _provider.Store("default", _provider.New(4444));
            _provider.Store("default", _provider.New(55555));

            
            Assert.That(SearchBuilder.Count(), Is.EqualTo(5));
            Assert.That(SearchBuilder.Slice(0, 3).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.Slice(1, 3).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.Slice(3, 3).Count(), Is.EqualTo(2));

            // Count() and Search() should return the same results
            Assert.That(SearchBuilder.Search().Count(), Is.EqualTo(5));
            Assert.That(SearchBuilder.Slice(0, 3).Search().Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.Slice(1, 3).Search().Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.Slice(3, 3).Search().Count(), Is.EqualTo(2));
        }

        [Test]
        public void ShouldSortByRelevance() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "michael is in the kitchen").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "michael as a cousin named michel").Analyze());
            _provider.Store("default", _provider.New(3).Add("body", "speak inside the mic").Analyze());
            _provider.Store("default", _provider.New(4).Add("body", "a dog is pursuing a cat").Analyze());
            _provider.Store("default", _provider.New(5).Add("body", "the elephant can't catch up the dog").Analyze());

            var michael = SearchBuilder.WithField("body", "michael").Search().ToList();
            Assert.That(michael.Count(), Is.EqualTo(2));
            Assert.That(michael[0].Score >= michael[1].Score, Is.True);

            // Sorting on score is always descending
            michael = SearchBuilder.WithField("body", "michael").Ascending().Search().ToList();
            Assert.That(michael.Count(), Is.EqualTo(2));
            Assert.That(michael[0].Score >= michael[1].Score, Is.True);
        }

        [Test]
        public void ShouldSortByDate() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("date", new DateTime(2010, 05, 28, 12, 30, 30)));
            _provider.Store("default", _provider.New(2).Add("date", DateTime.MinValue));
            _provider.Store("default", _provider.New(3).Add("date", DateTime.MaxValue));

            var date = SearchBuilder.SortByDateTime("date").Search().ToList();
            Assert.That(date.Count(), Is.EqualTo(3));
            Assert.That(date[0].ContentItemId, Is.EqualTo(3));
            Assert.That(date[1].ContentItemId, Is.EqualTo(1));
            Assert.That(date[2].ContentItemId, Is.EqualTo(2));

            date = SearchBuilder.SortByDateTime("date").Ascending().Search().ToList();
            Assert.That(date[0].ContentItemId, Is.EqualTo(2));
            Assert.That(date[1].ContentItemId, Is.EqualTo(1));
            Assert.That(date[2].ContentItemId, Is.EqualTo(3));
        }

        [Test]
        public void ShouldSortByNumber() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("downloads", 111));
            _provider.Store("default", _provider.New(2).Add("downloads", int.MaxValue));
            _provider.Store("default", _provider.New(3).Add("downloads", int.MinValue));

            var number = SearchBuilder.SortByInteger("downloads").Search().ToList();
            Assert.That(number.Count(), Is.EqualTo(3));
            Assert.That(number[0].ContentItemId, Is.EqualTo(2));
            Assert.That(number[1].ContentItemId, Is.EqualTo(1));
            Assert.That(number[2].ContentItemId, Is.EqualTo(3));

            number = SearchBuilder.SortByInteger("downloads").Ascending().Search().ToList();
            Assert.That(number.Count(), Is.EqualTo(3));
            Assert.That(number[0].ContentItemId, Is.EqualTo(3));
            Assert.That(number[1].ContentItemId, Is.EqualTo(1));
            Assert.That(number[2].ContentItemId, Is.EqualTo(2));
        }

        [Test]
        public void ShouldSortByBoolean() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("active", true));
            _provider.Store("default", _provider.New(2).Add("active", false));

            var number = SearchBuilder.SortByBoolean("active").Search().ToList();
            Assert.That(number.Count(), Is.EqualTo(2));
            Assert.That(number[0].ContentItemId, Is.EqualTo(1));
            Assert.That(number[1].ContentItemId, Is.EqualTo(2));

            number = SearchBuilder.SortByBoolean("active").Ascending().Search().ToList();
            Assert.That(number.Count(), Is.EqualTo(2));
            Assert.That(number[0].ContentItemId, Is.EqualTo(2));
            Assert.That(number[1].ContentItemId, Is.EqualTo(1));
        }

        [Test]
        public void ShouldSortByDouble() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("rating", 111.111));
            _provider.Store("default", _provider.New(2).Add("rating", double.MaxValue));
            _provider.Store("default", _provider.New(3).Add("rating", double.MinValue));

            var number = SearchBuilder.SortByDouble("rating").Search().ToList();
            Assert.That(number.Count(), Is.EqualTo(3));
            Assert.That(number[0].ContentItemId, Is.EqualTo(2));
            Assert.That(number[1].ContentItemId, Is.EqualTo(1));
            Assert.That(number[2].ContentItemId, Is.EqualTo(3));

            number = SearchBuilder.SortByDouble("rating").Ascending().Search().ToList();
            Assert.That(number.Count(), Is.EqualTo(3));
            Assert.That(number[0].ContentItemId, Is.EqualTo(3));
            Assert.That(number[1].ContentItemId, Is.EqualTo(1));
            Assert.That(number[2].ContentItemId, Is.EqualTo(2));
        }

        [Test]
        public void ShouldEscapeSpecialChars() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Orchard has been developped in C#").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Windows has been developped in C++").Analyze());

            var cs = SearchBuilder.Parse("body", "C#").Search().ToList();
            Assert.That(cs.Count(), Is.EqualTo(2));

            var cpp = SearchBuilder.Parse("body", "C++").Search().ToList();
            Assert.That(cpp.Count(), Is.EqualTo(2));

        }

        [Test]
        public void ShouldHandleMandatoryFields() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Orchard has been developped in C#").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Windows has been developped in C++").Analyze());

            Assert.That(SearchBuilder.WithField("body", "develop").Search().ToList().Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "develop").WithField("body", "Orchard").Search().ToList().Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "develop").WithField("body", "Orchard").Mandatory().Search().ToList().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("body", "develop").WithField("body", "Orchard").Mandatory().Search().First().ContentItemId, Is.EqualTo(1));
        }

        [Test]
        public void ShouldHandleForbiddenFields() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Orchard has been developped in C#").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Windows has been developped in C++").Analyze());

            Assert.That(SearchBuilder.WithField("body", "developped").Search().ToList().Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "developped").WithField("body", "Orchard").Search().ToList().Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "developped").WithField("body", "Orchard").Forbidden().Search().ToList().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("body", "developped").WithField("body", "Orchard").Forbidden().Search().First().ContentItemId, Is.EqualTo(2));
        }

        [Test]
        public void ShouldHandleWeight() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Orchard has been developped in C#").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Windows has been developped in C++").Analyze());

            Assert.That(SearchBuilder.WithField("body", "developped").WithField("body", "Orchard").Weighted(2).Search().First().ContentItemId, Is.EqualTo(1));
        }

        [Test]
        public void ShouldParseLuceneQueries() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Bradley is in the kitchen.").Analyze().Add("title", "Beer and takos").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Renaud is also in the kitchen.").Analyze().Add("title", "A love affair").Analyze());
            _provider.Store("default", _provider.New(3).Add("body", "Bertrand is a little bit jealous.").Analyze().Add("title", "Soap opera").Analyze());

            Assert.That(SearchBuilder.Parse(new[] {"body"}, "kitchen", false).Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.Parse(new[] {"body"}, "kitchen bertrand", false).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.Parse(new[] {"body"}, "kitchen +bertrand", false).Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.Parse(new[] {"body"}, "+kitchen +bertrand", false).Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.Parse(new[] {"body"}, "kit*", false).Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.Parse(new[] {"body", "title"}, "bradley love^3 soap", false).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.Parse(new[] {"body", "title"}, "bradley love^3 soap", false).Search().First().ContentItemId, Is.EqualTo(2));
        }

        [Test]
        public void ParseQueriesArePrefixedByDefault() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Bradley is in the kitchen.").Analyze().Add("title", "Beer and takos").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Renaud is also in the kitchen.").Analyze().Add("title", "A love affair").Analyze());
            _provider.Store("default", _provider.New(3).Add("body", "Bertrand is a little bit jealous.").Analyze().Add("title", "Soap opera").Analyze());

            // a prefix is added to the clause
            Assert.That(SearchBuilder.Parse(new[] { "body" }, "kit", false).Count(), Is.EqualTo(2));

            // ExactMatch prevents a prefix to be added
            Assert.That(SearchBuilder.Parse(new[] { "body" }, "kit", false).ExactMatch().Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldParseLuceneQueriesWithSpecificFields() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Bradley is in the kitchen.").Analyze().Add("title", "Beer and takos").Analyze());
            _provider.Store("default", _provider.New(2).Add("body", "Renaud is also in the kitchen.").Analyze().Add("title", "A love affair").Analyze());
            _provider.Store("default", _provider.New(3).Add("body", "Bertrand is a little bit jealous.").Analyze().Add("title", "Soap opera").Analyze());

            // specifying a field to match
            Assert.That(SearchBuilder.Parse(new[] { "body" }, "title:bradley", false).Search().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.Parse(new[] { "body" }, "title:s*", false).Search().Count(), Is.EqualTo(1));

            // checking terms fall back to the default fields
            Assert.That(SearchBuilder.Parse(new[] { "body" }, "title:bradley bradley", false).Search().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ShouldFilterIntValues() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("field", 1));
            _provider.Store("default", _provider.New(2).Add("field", 22));
            _provider.Store("default", _provider.New(3).Add("field", 333));

            Assert.That(SearchBuilder.WithField("field", 1).ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("field", 22).ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("field", 333).ExactMatch().Count(), Is.EqualTo(1));

            Assert.That(SearchBuilder.WithField("field", 0).ExactMatch().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("field", 2).ExactMatch().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("field", 3).ExactMatch().Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldFilterStoredIntValues() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("field", 1).Store());
            _provider.Store("default", _provider.New(2).Add("field", 22).Store());
            _provider.Store("default", _provider.New(3).Add("field", 333).Store());

            Assert.That(SearchBuilder.WithField("field", 1).ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("field", 22).ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("field", 333).ExactMatch().Count(), Is.EqualTo(1));

            Assert.That(SearchBuilder.WithField("field", 0).ExactMatch().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("field", 2).ExactMatch().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("field", 3).ExactMatch().Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldProvideAvailableFields() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("a", "Home").Analyze());
            _provider.Store("default", _provider.New(2).Add("b", DateTime.Now).Store());
            _provider.Store("default", _provider.New(3).Add("c", 333));

            Assert.That(_provider.GetFields("default").Count(), Is.EqualTo(4));
            Assert.That(_provider.GetFields("default").OrderBy(s => s).ToArray(), Is.EqualTo(new [] { "a", "b", "c", "id"}));
        }

        [Test]
        public void FiltersShouldNotAlterResults() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "Orchard has been developped by Microsoft in C#").Analyze().Add("culture", 1033));
            _provider.Store("default", _provider.New(2).Add("body", "Windows a été développé par Microsoft en C++").Analyze().Add("culture", 1036));
            _provider.Store("default", _provider.New(3).Add("title", "Home").Analyze().Add("culture", 1033));

            Assert.That(SearchBuilder.WithField("body", "Microsoft").Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "Microsoft").WithField("culture", 1033).Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.WithField("body", "Microsoft").WithField("culture", 1033).AsFilter().Count(), Is.EqualTo(1));
            
            Assert.That(SearchBuilder.WithField("body", "Orchard").WithField("culture", 1036).Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "Orchard").WithField("culture", 1036).AsFilter().Count(), Is.EqualTo(0));

            Assert.That(SearchBuilder.WithField("culture", 1033).Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("culture", 1033).AsFilter().Count(), Is.EqualTo(2));
            
            Assert.That(SearchBuilder.WithField("body", "blabla").WithField("culture", 1033).Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("body", "blabla").WithField("culture", 1033).AsFilter().Count(), Is.EqualTo(0));

            Assert.That(SearchBuilder.Parse("title", "home").Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.Parse("title", "home").WithField("culture", 1033).AsFilter().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ParsedTextShouldBeEscapedByDefault() {
            _provider.CreateIndex("default");
            _provider.Store("default", _provider.New(1).Add("body", "foo.bar").Analyze());

            Assert.That(SearchBuilder.Parse("body", "*@!woo*@!").Count(), Is.EqualTo(0));
        }

        [Test]
        public void FieldsCanContainMultipleValue() {
            _provider.CreateIndex("default");
            var documentIndex = _provider.New(1)
                .Add("tag-id", 1)
                .Add("tag-id", 2)
                .Add("tag-id", 3)
                .Add("tag-value", "tag1")
                .Add("tag-value", "tag2")
                .Add("tag-value", "tag3");

            _provider.Store("default", documentIndex);

            Assert.That(SearchBuilder.WithField("tag-id", 0).Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("tag-id", 1).Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-id", 2).Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-id", 3).Count(), Is.EqualTo(1));

            Assert.That(SearchBuilder.WithField("tag-value", "tag").ExactMatch().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("tag-value", "tag1").ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-value", "tag2").ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-value", "tag3").ExactMatch().Count(), Is.EqualTo(1));

            Assert.That(SearchBuilder.WithField("tag-value", "tag").Count(), Is.EqualTo(1));
        }

        [Test]
        public void AnalyzedFieldsAreNotCaseSensitive() {
            _provider.CreateIndex("default");
            var documentIndex = _provider.New(1)
                .Add("tag-id", 1)
                .Add("tag-value", "Tag1").Analyze();

            _provider.Store("default", documentIndex);

            // trying in prefix mode
            Assert.That(SearchBuilder.WithField("tag-value", "tag").Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-value", "Tag").Count(), Is.EqualTo(1));

            // trying in full word match mode
            Assert.That(SearchBuilder.WithField("tag-value", "tag1").ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-value", "Tag1").ExactMatch().Count(), Is.EqualTo(1));
        }

        [Test]
        public void NotAnalyzedFieldsAreSearchable() {
            _provider.CreateIndex("default");
            var documentIndex = _provider.New(1)
                .Add("tag-id", 1)
                .Add("tag-valueL", "tag1")
                .Add("tag-valueU", "Tag1");

            _provider.Store("default", documentIndex);

            // a value which is not analyzed, is not lowered cased in the index
            Assert.That(SearchBuilder.WithField("tag-valueL", "tag").Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-valueU", "tag").Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("tag-valueL", "Tag").Count(), Is.EqualTo(1)); // queried term is lower cased
            Assert.That(SearchBuilder.WithField("tag-valueU", "Tag").Count(), Is.EqualTo(0)); // queried term is lower cased
            Assert.That(SearchBuilder.WithField("tag-valueL", "tag1").ExactMatch().Count(), Is.EqualTo(1));
            Assert.That(SearchBuilder.WithField("tag-valueU", "tag1").ExactMatch().Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldReturnAllDocuments() {
            _provider.CreateIndex("default");
            for(var i = 1; i<100;i++) {
                _provider.Store("default", _provider.New(i).Add("term-id", i).Store());
            }

            Assert.That(SearchBuilder.Count(), Is.EqualTo(99));
        }

        [Test]
        public void NoClauseButAFilter() {
            _provider.CreateIndex("default");
            for (var i = 1; i < 50; i++) {
                _provider.Store("default", _provider.New(i).Add("term-id", i / 10).Store());
            }

            Assert.That(SearchBuilder.Count(), Is.EqualTo(49));
            Assert.That(SearchBuilder.WithField("term-id", 0).ExactMatch().AsFilter().Count(), Is.EqualTo(9));
            Assert.That(SearchBuilder.WithField("term-id", 1).ExactMatch().AsFilter().Count(), Is.EqualTo(10));
            Assert.That(SearchBuilder.WithField("term-id", 2).ExactMatch().AsFilter().Count(), Is.EqualTo(10));
            Assert.That(SearchBuilder.WithField("term-id", 3).ExactMatch().AsFilter().Count(), Is.EqualTo(10));
            Assert.That(SearchBuilder.WithField("term-id", 4).ExactMatch().AsFilter().Count(), Is.EqualTo(10));
        }

        [Test]
        public void MandatoryCanBeUsedrMultipleTimes() {
            _provider.CreateIndex("default");
            _provider.Store("default",
                _provider.New(1)
                    .Add("field1", 1)
                    .Add("field2", 1)
                    .Add("field3", 1)
            );

            _provider.Store("default",
                _provider.New(2)
                    .Add("field1", 1)
                    .Add("field2", 2)
                    .Add("field3", 2)
            );

            _provider.Store("default",
                _provider.New(3)
                    .Add("field1", 1)
                    .Add("field2", 2)
                    .Add("field3", 3)
            );


            Assert.That(SearchBuilder.WithField("field1", 0).Mandatory().Count(), Is.EqualTo(0));
            Assert.That(SearchBuilder.WithField("field1", 1).Mandatory().Count(), Is.EqualTo(3));
            Assert.That(SearchBuilder.WithField("field1", 1).Mandatory().WithField("field2", 2).Mandatory().Count(), Is.EqualTo(2));
            Assert.That(SearchBuilder.WithField("field1", 1).Mandatory().WithField("field2", 2).Mandatory().WithField("field3", 3).Mandatory().Count(), Is.EqualTo(1));
        }

        [Test]
        public void SearchQueryCanContainMultipleFilters() {
            _provider.CreateIndex("default");
            _provider.Store("default",
                _provider.New(1)
                    .Add("field1", 1)
                    .Add("field2", 1)
                    .Add("field3", 1)
            );

            _provider.Store("default",
                _provider.New(2)
                    .Add("field1", 1)
                    .Add("field2", 2)
                    .Add("field3", 2)
            );

            _provider.Store("default",
                _provider.New(3)
                    .Add("field1", 1)
                    .Add("field2", 2)
                    .Add("field3", 3)
            );

            Assert.That(SearchBuilder.WithField("field1", 1).Count(), Is.EqualTo(3));

            Assert.That(SearchBuilder
                .WithField("field1", 1)
                .WithField("field2", 2).AsFilter()
                .Count(), Is.EqualTo(2));

            Assert.That(SearchBuilder
                .WithField("field1", 1)
                .WithField("field2", 2).Mandatory().AsFilter()
                .WithField("field3", 3).Mandatory().AsFilter()
                .Count(), Is.EqualTo(1));
        }
    }
}
