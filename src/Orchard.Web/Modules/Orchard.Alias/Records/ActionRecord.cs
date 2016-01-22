namespace Orchard.Alias.Records {
    public class ActionRecord {
        public virtual int Id { get; set; }
        public virtual string Area { get; set; }
        public virtual string Controller { get; set; }
        public virtual string Action { get; set; }
    }
}