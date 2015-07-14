using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Recipes.Services {
    public interface IRecipeBuilderStep : IDependency {
        string Name { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }
        int Priority { get; }
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void Build(BuildContext context);
    }
}