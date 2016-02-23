using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Recipes.Services;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers {
    [TestFixture]
    public class RecipeParserTest {
        protected IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<RecipeParser>().As<IRecipeParser>();

            _container = builder.Build();
        }

        [Test]
        public void ParsingRecipeYieldsUniqueIdsForSteps() {
            var recipeText = @"<Orchard><Foo /><Bar /><Baz /></Orchard>";
            var recipeParser = _container.Resolve<IRecipeParser>();
            var recipe = recipeParser.ParseRecipe(recipeText);
            
            // Assert that each step has a unique ID.
            Assert.IsTrue(recipe.RecipeSteps.GroupBy(x => x.Id).All(y => y.Count() == 1));
        }
    }
}
