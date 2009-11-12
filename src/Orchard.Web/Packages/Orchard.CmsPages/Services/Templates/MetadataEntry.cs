namespace Orchard.CmsPages.Services.Templates {
    public class MetadataEntry {
        public string Tag { get; set; }
        public string Value { get; set; }

        public override string ToString() {
            return string.Format("Tag:{0}, Value:{1}", Tag, Value);
        }
    }
}