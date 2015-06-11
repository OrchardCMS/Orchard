namespace Orchard.Azure.MediaServices.Models.Jobs {
    public enum JobStatus {
        Pending,
        Processing,
        Finished,
        Canceling,
        Canceled,
        Queued,
		Scheduled,
		Faulted,
        Archived
    }
}