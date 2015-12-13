namespace Orchard.MediaProcessing.Models {
    public class FileNameRecord {
        public virtual int Id { get; set; }
        public virtual string Path { get; set; }
        public virtual string FileName { get; set; }

        // Parent property
        public virtual ImageProfilePartRecord ImageProfilePartRecord { get; set; }
    }
}