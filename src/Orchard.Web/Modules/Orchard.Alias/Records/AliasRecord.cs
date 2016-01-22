namespace Orchard.Alias.Records {
    public class AliasRecord {
        public virtual int Id { get; set; }
        public virtual string Path { get; set; }
        public virtual ActionRecord Action { get; set; }
        public virtual string RouteValues { get; set; }
        public virtual string Source { get; set; }
    }
}