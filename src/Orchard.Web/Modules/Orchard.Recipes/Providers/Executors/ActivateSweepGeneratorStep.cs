using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Tasks;

namespace Orchard.Recipes.Providers.Executors {
    public class ActivateSweepGeneratorStep : RecipeExecutionStep {
        private readonly ISweepGenerator _sweepGenerator;

        public ActivateSweepGeneratorStep(ISweepGenerator sweepGenerator, RecipeExecutionLogger logger) 
            : base(logger) {
            _sweepGenerator = sweepGenerator;
        }

        public override string Name { get { return "ActivateSweepGenerator"; } }

        public override void Execute(RecipeExecutionContext context) {
            _sweepGenerator.Activate();
        }
    }
}
