namespace Orchard.ContentManagement {
    public interface IContentManagerSession : IDependency {
        void Store(ContentItem item);
        bool RecallVersionRecordId(int id, out ContentItem item);
    }
}
