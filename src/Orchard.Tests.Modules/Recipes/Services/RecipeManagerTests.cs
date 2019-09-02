using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.WebSite;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Services;
using Orchard.Tests.Environment.Extensions;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Recipes.Services {
    [TestFixture]
    public class RecipeManagerTests : DatabaseEnabledTestsBase {
        private IRecipeManager _recipeManager;
        private IRecipeHarvester _recipeHarvester;
        private IRecipeParser _recipeParser;
        private IExtensionFolders _folders;

        private const string DataPrefix = "Orchard.Tests.Modules.Recipes.Services.FoldersData.";
        private string _tempFolderName;

        protected override IEnumerable<Type> DatabaseTypes {
            get { yield return typeof (RecipeStepResultRecord); }
        }

        public override void Register(ContainerBuilder builder) {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().Assembly;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.StartsWith(DataPrefix))
                {
                    string text;
                    using (var stream = assembly.GetManifestResourceStream(name))
                    {
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
                    using (var stream = new FileStream(targetPath, FileMode.Create))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(text);
                        }
                    }
                }
            }

            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(), new Mock<ICriticalErrorProvider>().Object);
            _folders = new ModuleFolders(new[] { _tempFolderName }, harvester);
            builder.RegisterType<RecipeManager>().As<IRecipeManager>();
            builder.RegisterType<RecipeHarvester>().As<IRecipeHarvester>();
            builder.RegisterType<RecipeStepExecutor>().As<IRecipeStepExecutor>();
            builder.RegisterType<StubStepQueue>().As<IRecipeStepQueue>().InstancePerLifetimeScope();
            builder.RegisterType<StubRecipeScheduler>().As<IRecipeScheduler>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterInstance(new Mock<IRecipeExecuteEventHandler>().Object);
            builder.RegisterType<ExtensionManagerTests.StubLoaders>().As<IExtensionLoader>();
            builder.RegisterType<RecipeParser>().As<IRecipeParser>();
            builder.RegisterType<StubWebSiteFolder>().As<IWebSiteFolder>();
            builder.RegisterType<CustomRecipeHandler>().As<IRecipeHandler>();
        }

        public override void Init() {
            base.Init();

            _recipeManager = _container.Resolve<IRecipeManager>();
            _recipeParser = _container.Resolve<IRecipeParser>();
            _recipeHarvester = _container.Resolve<IRecipeHarvester>();
        }

        public override void Cleanup() {
            Directory.Delete(_tempFolderName, true);
            base.Cleanup();
        }

        [Test]
        public void HarvestRecipesFailsToFindRecipesWhenCalledWithNotExistingExtension() {
            var recipes = _recipeHarvester.HarvestRecipes("cantfindme");

            Assert.That(recipes.Count(), Is.EqualTo(0));
        }

        [Test]
        public void HarvestRecipesShouldHarvestRecipeXmlFiles() {
            var recipes = _recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ParseRecipeLoadsRecipeMetaDataIntoModel() {
            var recipes = _recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count(), Is.EqualTo(1));

            var sampleRecipe = recipes.First();
            Assert.That(sampleRecipe.Name, Is.EqualTo("cms"));
            Assert.That(sampleRecipe.Description, Is.EqualTo("a sample Orchard recipe describing a cms"));
            Assert.That(sampleRecipe.Author, Is.EqualTo("orchard"));
            Assert.That(sampleRecipe.Version, Is.EqualTo("1.1"));
            Assert.That(sampleRecipe.IsSetupRecipe, Is.True);
            Assert.That(sampleRecipe.WebSite, Is.EqualTo("http://orchardproject.net"));
            Assert.That(sampleRecipe.Tags, Is.EqualTo("tag1, tag2"));
        }

        [Test]
        public void ParseRecipeLoadsRecipeStepsIntoModel() {
            var recipes = (List<Recipe>)_recipeHarvester.HarvestRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));

            var sampleRecipe = recipes[0];
            var recipeSteps = (List<RecipeStep>)sampleRecipe.RecipeSteps;

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

        [Test]
        public void ExecuteUpdatesStepResults()
        {
            var recipes = (List<Recipe>)_recipeHarvester.HarvestRecipes("Sample1");
            var sampleRecipe = recipes.First();
            var steps = sampleRecipe.RecipeSteps.ToArray();

            _recipeManager.Execute(sampleRecipe);

            var stepResultRepository = _container.Resolve<IRepository<RecipeStepResultRecord>>();
            var stepResults = stepResultRepository.Table.ToArray();

            Assert.That(stepResults.Count(), Is.EqualTo(steps.Count()));
            Assert.IsTrue(stepResults.All(x => x.IsCompleted));
        }

        [Test]
        public void CanExecuteSameStepMultipleTimes()
        {
            var recipes = (List<Recipe>)_recipeHarvester.HarvestRecipes("Sample2");
            var recipe = recipes.Single(x => x.Name == "Duplicate Steps");
            var steps = recipe.RecipeSteps.ToArray();

            _recipeManager.Execute(recipe);

            var stepResultRepository = _container.Resolve<IRepository<RecipeStepResultRecord>>();
            var stepResults = stepResultRepository.Table.ToArray();

            Assert.That(stepResults.Count(), Is.EqualTo(steps.Count()));
            Assert.IsTrue(stepResults.All(x => x.IsCompleted));
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
        public string[] _handles = { "Module", "Theme", "Migration", "Custom1", "Custom2", "Command", "Metadata", "Feature", "Settings", "Recipes" };

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