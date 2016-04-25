using System;
using System.Collections.Generic;
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
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override string Name {
            get { return "MemberBindings"; }
        }

        public override void Execute(RecipeExecutionContext context) {

            foreach (var memberBindingElement in context.RecipeStep.Step.Elements()) {                
                Logger.Information("Importing member bindings.");

                try {
                    _repository.Create(new MemberBindingRecord {
                    Member = memberBindingElement.Attribute("Member").Value,
                    Type = memberBindingElement.Attribute("Type").Value,
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
