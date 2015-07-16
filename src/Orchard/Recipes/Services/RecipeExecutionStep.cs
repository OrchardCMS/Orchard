using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public abstract class RecipeExecutionStep : Component, IRecipeExecutionStep {
        public abstract string Name { get; }

        public virtual IEnumerable<string> Names {
            get { yield return Name; }
        }

        public virtual LocalizedString DisplayName {
            get { return T(Name); }
        }

        public virtual LocalizedString Description {
            get { return DisplayName; }
        }

        protected virtual string Prefix {
            get { return GetType().Name; }
        }

        public virtual dynamic BuildEditor(dynamic shapeFactory) {
            return null;
        }

        public virtual dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            return null;
        }

        public virtual void UpdateStep(UpdateRecipeExecutionStepContext context) {
        }

        public abstract void Execute(RecipeExecutionContext context);
    }
}