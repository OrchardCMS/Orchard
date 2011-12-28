using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.WebSite;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Recipes.Services {
    [TestFixture]
    public class RecipeManagerTests {
        private IContainer _container;
        private IRecipeManager _recipeManager;
        private IRecipeHarvester _recipeHarvester;
        private IRecipeParser _recipeParser;
        private IExtensionFolders _folders;

        private const string DataPrefix = "Orchard.Tests.Modules.Recipes.Services.FoldersData.";
        private string _tempFolderName;

        [SetUp]
        public void Init() {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().Assembly;
            foreach (var name in assembly.GetManifestResourceNames()) {
                if (name.StartsWith(DataPrefix)) {
                    string text;
                    using (var stream = assembly.GetManifestResourceStream(name)) {
                        using (var reader = new StreamReader(stream))
                            text = reader.ReadToEnd();

                    }

                    // Pro filtering
                    var relativePath = name
                        .Substring(DataPrefix.Length)
                        .Replace(".txt", ":txt")
                        .Replace(".recipe.xml", ":recipe:xml")
                        .Replace('.', Path.DirectorySeparatorChar)
                        .Replace(":txt", ".txt")
                        .Replace(":recipe:xml", ".recipe.xml");

                    var targetPath = Path.Combine(_tempFolderName, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    using (var stream = new FileStream(targetPath, FileMode.Create)) {
                        using (var writer = new StreamWriter(stream)) {
                            writer.Write(text);
                        }
                    }
                }
            }

            var builder = new ContainerBuilder();
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(), new Mock<ICriticalErrorProvider>().Object);
            _folders = new ModuleFolders(new[] { _tempFolderName }, harvester);
            builder.RegisterType<RecipeManager>().As<IRecipeManager>();
            builder.RegisterType<RecipeHarvester>().As<IRecipeHarvester>();
            builder.RegisterType<RecipeStepExecutor>().As<IRecipeStepExecutor>();
            builder.RegisterType<StubStepQueue>().As<IRecipeStepQueue>().InstancePerLifetimeScope();
            builder.RegisterType<StubRecipeJournal>().As<IRecipeJournal>();
            builder.RegisterType<StubRecipeScheduler>().As<IRecipeScheduler>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<Environment.Extensions.ExtensionManagerTests.StubLoaders>().As<IExtensionLoader>();
            builder.RegisterType<RecipeParser>().As<IRecipeParser>();
            builder.RegisterType<StubWebSiteFolder>().As<IWebSiteFolder>();
            builder.RegisterType<CustomRecipeHandler>().As<IRecipeHandler>();

            _container = builder.Build();
            _recipeManager = _container.Resolve<IRecipeManager>();
            _recipeParser = _container.Resolve<IRecipeParser>();
            _recipeHarvester = _container.Resolve<IRecipeHarvester>();
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolderName, true);
        }

        [Test]
        public void HarvestRecipesFailsToFindRecipesWhenCalledWithNotExistingExtension() {
            var recipes = (List<Recipe>) _recipeHarvester.HarvestRecipes("cantfindme");

            Assert.That(recipes.Count, Is.EqualTo(0));
        }

        [Test]
        public void HarvestRecipesShouldHarvestRecipeXmlFiles() {
            var recipes = (List<Recipe>)_recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));
        }

        [Test]
        public void ParseRecipeLoadsRecipeMetaDataIntoModel() {
            var recipes = (List<Recipe>) _recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));

            var sampleRecipe = recipes[0];
            Assert.That(sampleRecipe.Name, Is.EqualTo("cms"));
            Assert.That(sampleRecipe.Description, Is.EqualTo("a sample Orchard recipe describing a cms"));
            Assert.That(sampleRecipe.Author, Is.EqualTo("orchard"));
            Assert.That(sampleRecipe.Version, Is.EqualTo("1.1"));
            Assert.That(sampleRecipe.WebSite, Is.EqualTo("http://orchardproject.net"));
            Assert.That(sampleRecipe.Tags, Is.EqualTo("tag1, tag2"));
        }

        [Test]
        public void ParseRecipeLoadsRecipeStepsIntoModel() {
            var recipes = (List<Recipe>)_recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));

            var sampleRecipe = recipes[0];
            var recipeSteps = (List<RecipeStep>) sampleRecipe.RecipeSteps;

            Assert.That(recipeSteps.Count, Is.EqualTo(9));
        }

        [Test]
        public void ParseRecipeThrowsOnInvalidXml() {
            Assert.Throws(typeof(XmlException), () => _recipeParser.ParseRecipe("<reipe></recipe>"));
        }

        [Test]
        public void ExecuteInvokesHandlersWithSteps() {
            var recipes = (List<Recipe>)_recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));

            var sampleRecipe = recipes[0];
            _recipeManager.Execute(sampleRecipe);

            Assert.That(CustomRecipeHandler.AttributeValue == "value1");
        }
    }

    public class StubRecipeJournal : IRecipeJournal {
        public void ExecutionStart(string executionId) {
        }

        public void ExecutionComplete(string executionId) {
        }

        public void ExecutionFailed(string executionId) {
        }

        public void WriteJournalEntry(string executionId, string message) {
        }

        public RecipeJournal GetRecipeJournal(string executionId) {
            return new RecipeJournal();
        }

        public RecipeStatus GetRecipeStatus(string executionId) {
            return RecipeStatus.Complete;
        }
    }

    public class StubStepQueue : IRecipeStepQueue {
        readonly Queue<RecipeStep> _queue = new Queue<RecipeStep>();

        public void Enqueue(string executionId, RecipeStep step) {
            _queue.Enqueue(step);
        }

        public RecipeStep Dequeue(string executionId) {
            return _queue.Count == 0 ? null : _queue.Dequeue();
        }
    }

    public class StubRecipeScheduler : IRecipeScheduler {
        private readonly IRecipeStepExecutor _recipeStepExecutor;

        public StubRecipeScheduler(IRecipeStepExecutor recipeStepExecutor) {
            _recipeStepExecutor = recipeStepExecutor;
        }

        public void ScheduleWork(string executionId) {
            while (_recipeStepExecutor.ExecuteNextStep(executionId)) ;
        }
    }

    public class CustomRecipeHandler : IRecipeHandler {
        public static string AttributeValue;
        public string[] _handles = {"Module", "Theme", "Migration", "Custom1", "Custom2", "Command", "Metadata", "Feature", "Settings"};

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (_handles.Contains(recipeContext.RecipeStep.Name)) {
                recipeContext.Executed = true;
            }
            if (recipeContext.RecipeStep.Name == "Custom1") {
                foreach (var attribute in recipeContext.RecipeStep.Step.Attributes().Where(attribute => attribute.Name == "attr1")) {
                    AttributeValue = attribute.Value;
                    recipeContext.Executed = true;
                }
            }
        }
    }
}