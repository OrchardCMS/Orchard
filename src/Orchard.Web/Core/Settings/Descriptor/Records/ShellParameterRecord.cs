namespace Orchard.Core.Settings.Descriptor.Records {
    public class ShellParameterRecord {
        public virtual int Id { get; set; }
        public virtual ShellDescriptorRecord ShellDescriptorRecord { get; set; }
        public virtual string Component { get; set; }
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
    }
}