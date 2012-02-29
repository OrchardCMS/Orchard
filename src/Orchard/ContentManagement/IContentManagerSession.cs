namespace Orchard.ContentManagement {
    public interface IContentManagerSession : IDependency {
        void Store(ContentItem item);
        bool RecallVersionRecordId(int id, out ContentItem item);
        bool RecallContentRecordId(int id, out ContentItem item);

        void Clear();
    }
}
