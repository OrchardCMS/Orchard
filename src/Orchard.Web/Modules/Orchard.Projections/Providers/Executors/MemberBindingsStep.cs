using System;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Projections.Models;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Projections.Providers.Executors {
    public class MemberBindingsStep : RecipeExecutionStep {
        private readonly IRepository<MemberBindingRecord> _repository;

        public MemberBindingsStep(IRepository<MemberBindingRecord> repository, RecipeExecutionLogger logger) : base(logger) {
            _repository = repository;
        }

        public override string Name {
            get { return "MemberBindings"; }
        }

        public override void Execute(RecipeExecutionContext context) {
            foreach (var memberBindingElement in context.RecipeStep.Step.Elements()) {
                Logger.Information("Importing member bindings.");
                try {
                    var member = memberBindingElement.Attribute("Member").Value;
                    var type = memberBindingElement.Attribute("Type").Value;
                    if (_repository.Get(b => b.Member == member && b.Type == type) != null)
                        continue;
                    _repository.Create(new MemberBindingRecord {
                        Member = member,
                        Type = type,
                        DisplayName = memberBindingElement.Attribute("DisplayName").Value,
                        Description = memberBindingElement.Attribute("Description").Value,
                    });
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error while importing member bindings.");
                    throw;
                }
            }
        }
    }
}