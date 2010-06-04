namespace Orchard.Core.Settings.Metadata.Records {
    public class ContentTypePartDefinitionRecord {
        public virtual int Id { get; set; }
        public virtual ContentPartDefinitionRecord ContentPartDefinitionRecord { get; set; }
        public virtual string Settings { get; set; }
    }
}
