namespace Orchard.Messaging.Models {
    public enum QueuedMessageStatus {
        Pending,
        Sending,
        Sent,
        Faulted
    }
}