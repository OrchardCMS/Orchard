namespace Orchard.Translations.Models {
    public class TranslatableRecord {
        public virtual int Id { get; set; }
        public static string Location { get; set; }
        public static string Value { get; set; }
    }
}