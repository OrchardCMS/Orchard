namespace Orchard.Core.Settings.Descriptor.Records {
    public class ShellFeatureRecord {
        public virtual int Id { get; set; }
        public virtual ShellDescriptorRecord ShellDescriptorRecord { get; set; }
        public virtual string Name { get; set; }
    }
}