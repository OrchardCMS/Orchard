namespace Orchard.Pages.Models {
    public static class ModelExtensions {
        public static bool IsPublished(this PageRevision revision) {
            if (revision != null &&
                revision.Page != null &&
                revision.Page.Published != null) {
                return ReferenceEquals(revision.Page.Published, revision);
            }
            return false;
        }
    }
}
