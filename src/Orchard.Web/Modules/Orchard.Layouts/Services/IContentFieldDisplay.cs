using Orchard.ContentManagement;

namespace Orchard.Layouts.Services {
    public interface IContentFieldDisplay : IDependency {
        dynamic BuildDisplay(IContent content, ContentField field, string displayType = "", string groupId = "");
        dynamic BuildEditor(IContent content, ContentField field, string groupId = "");
        dynamic UpdateEditor(IContent content, ContentField field, IUpdateModel updater, string groupId = "");
    }
}