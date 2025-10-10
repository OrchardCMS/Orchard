using Orchard.ContentManagement;

namespace Orchard.CustomForms.Services {
    public interface IEditorBuilderWrapper : IDependency {
        dynamic BuildEditor(IContent content);
        dynamic UpdateEditor(IContent content, IUpdateModel updateModel);
    }
}
