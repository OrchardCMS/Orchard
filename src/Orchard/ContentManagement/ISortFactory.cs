namespace Orchard.ContentManagement {
    public interface ISortFactory {
        ISortFactory WithRecord(string recordName);
        ISortFactory WithVersionRecord(string recordName);

        void Asc(string propertyName);
        void Desc(string propertyName);
    }
}
