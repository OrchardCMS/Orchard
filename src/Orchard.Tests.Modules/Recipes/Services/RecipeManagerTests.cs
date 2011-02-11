using System.Collections.Generic;
using System.IO;
using System.Xml;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.WebSite;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Recipes.Services {
    [TestFixture]
    public class RecipeManagerTests {
        private IContainer _container;
        private IRecipeManager _recipeManager;
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
            _folders = new ModuleFolders(new[] { _tempFolderName }, new StubCacheManager(), new StubWebSiteFolder());
            builder.RegisterType<RecipeManager>().As<IRecipeManager>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<Environment.Extensions.ExtensionManagerTests.StubLoaders>().As<IExtensionLoader>();
            builder.RegisterType<RecipeParser>().As<IRecipeParser>();
            builder.RegisterType<StubWebSiteFolder>().As<IWebSiteFolder>();

            _container = builder.Build();
            _recipeManager = _container.Resolve<IRecipeManager>();
            _recipeParser = _container.Resolve<IRecipeParser>();
        }

        [TearDown]
        public void Term() {
            Directory.Delete(_tempFolderName, true);
        }

        [Test]
        public void DiscoverRecipesFailsToFindRecipesWhenCalledWithNotExistingExtension() {
            var recipes = (List<Recipe>) _recipeManager.DiscoverRecipes("cantfindme");

            Assert.That(recipes.Count, Is.EqualTo(0));
        }

        [Test]
        public void DiscoverRecipesShouldDiscoverRecipeXmlFiles() {
            var recipes = (List<Recipe>)_recipeManager.DiscoverRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));
        }

        [Test]
        public void ParseRecipeLoadsRecipeMetaDataIntoModel() {
            var recipes = (List<Recipe>) _recipeManager.DiscoverRecipes("Sample1");
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
            var recipes = (List<Recipe>)_recipeManager.DiscoverRecipes("Sample1");
            Assert.That(recipes.Count, Is.EqualTo(1));

            var sampleRecipe = recipes[0];
            var recipeSteps = (List<RecipeStep>) sampleRecipe.RecipeSteps;

            Assert.That(recipeSteps.Count, Is.EqualTo(11));
        }

        [Test]
        public void ParseRecipeThrowsOnInvalidXml() {
            Assert.Throws(typeof(XmlException), () => _recipeParser.ParseRecipe("<reipe></recipe>"));
        }
    }
}