namespace Orchard.AuditTrail.Services.Models {
    public class Diff<T> {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }
}