namespace Orchard.Taxonomies.Models {
    /// <summary>
    /// Contrary to what its named states, it's not a part
    /// but a DTO to carry over a TermPart and a Field name
    /// </summary>
    public class TermContentItemPart {
        public string Field { get; set; }
        public TermPart TermPart { get; set; }
    }
}