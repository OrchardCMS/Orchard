namespace Orchard.Azure.MediaServices.Models.Assets {
    public class DisplayLocator {
        public DisplayLocator(string name, string id, string url) {
            Name = name;
            Id = id;
            Url = url;
        }

        public string Name { get; private set; }
        public string Id { get; private set; }
        public string Url { get; private set; }
    }
}