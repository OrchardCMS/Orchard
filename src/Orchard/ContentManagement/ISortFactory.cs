namespace Orchard.ContentManagement {
    public interface ISortFactory {
        ISortFactory WithRecord(string recordName);
        ISortFactory WithVersionRecord(string recordName);
        ISortFactory WithRelationship(string propertyName);

        void Asc(string propertyName);
        void Desc(string propertyName);
    }
}
