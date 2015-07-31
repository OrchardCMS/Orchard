using System.Xml.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Recipes.Models;
using Orchard.Recipes.Providers.RecipeHandlers;
using Orchard.Recipes.Services;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers {
    [TestFixture]
    public class RecipeExecutionStepHandlerTest {
        protected IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubRecipeExecutionStep>().As<IRecipeExecutionStep>().AsSelf().SingleInstance();
            builder.RegisterType<RecipeExecutionStepHandler>().SingleInstance();

            _container = builder.Build();
        }

        [Test]
        public void ExecuteRecipeExecutionStepHandlerTest() {
            var handlerUnderTest = _container.Resolve<RecipeExecutionStepHandler>();
            var fakeRecipeStep = _container.Resolve<StubRecipeExecutionStep>();

            var context = new RecipeContext {
                RecipeStep = new RecipeStep (id: "1", recipeName: "FakeRecipe",  name: "FakeRecipeStep", step: new XElement("FakeRecipeStep")),
                ExecutionId = "12345"
            };

            handlerUnderTest.ExecuteRecipeStep(context);

            Assert.That(fakeRecipeStep.IsExecuted, Is.True);
            Assert.That(context.Executed, Is.True);
        }
    }

    public class StubRecipeExecutionStep : RecipeExecutionStep {
        public override string Name {
            get { return "FakeRecipeStep"; }
        }

        public bool IsExecuted { get; set; }

        public override void Execute(RecipeExecutionContext context) {
            IsExecuted = true;
        }
    }
}
