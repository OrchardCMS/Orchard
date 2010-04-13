namespace Orchard.Extensions.Records {
    public class ExtensionRecord {
        public ExtensionRecord() {
// ReSharper disable DoNotCallOverridableMethodsInConstructor
            Enabled = true;
// ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool Enabled { get; set; }
    }
}