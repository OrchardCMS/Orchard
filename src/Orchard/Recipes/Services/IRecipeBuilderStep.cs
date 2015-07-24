using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services {
    public interface IRecipeBuilderStep : IDependency {
        string Name { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }

        /// <summary>
        /// The order in which this builder should execute.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The order in which this builder should be displayed.
        /// </summary>
        int Position { get; }
        bool IsVisible { get; }

        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void Configure(RecipeBuilderStepConfigurationContext configurationElement);
        void ConfigureDefault();
        void Build(BuildContext context);
    }
}