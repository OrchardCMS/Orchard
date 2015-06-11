
namespace Orchard.Tags.Models {
    public class TagCount {
        public TagCount() {
            Bucket = 1;
        }

        public string TagName { get; set; }
        public int Count { get; set; }
        public int Bucket { get; set; }
    }
}