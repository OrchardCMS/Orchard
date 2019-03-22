namespace Orchard.Indexing.Settings {
    public class FieldIndexing {
        public FieldIndexing() {
            Analyzed = true;
            TagsRemoved = true;
        }

        public bool Included { get; set; }
        public bool Stored { get; set; }
        public bool Analyzed { get; set; }
        public bool TagsRemoved { get; set; }
    }
}
