namespace Orchard.Projections.Models {
    public static class QueryPartRecordExtensions {
        public static string GetVersionedFieldIndexColumnName(this QueryPartRecord queryPartRecord) {
            return queryPartRecord == null ?
                QueryVersionScopeOptions.Published.ToVersionedFieldIndexColumnName() :
                queryPartRecord.VersionScope.ToVersionedFieldIndexColumnName();
        }
    }
}