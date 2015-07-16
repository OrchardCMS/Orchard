using Orchard.ContentManagement;
using Orchard.Localization;

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

        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void Build(BuildContext context);
    }
}