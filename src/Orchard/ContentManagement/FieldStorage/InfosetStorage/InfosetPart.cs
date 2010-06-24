namespace Orchard.ContentManagement.FieldStorage.InfosetStorage {
    public class InfosetPart : ContentPart {
        public InfosetPart() {
            Infoset = new Infoset();
            VersionInfoset = new Infoset();
        }

        public Infoset Infoset { get; set; }
        public Infoset VersionInfoset { get; set; }
    }
}