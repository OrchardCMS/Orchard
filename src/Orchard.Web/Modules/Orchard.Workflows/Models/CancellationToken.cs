namespace Orchard.Workflows.Models {
    public class CancellationToken {
        public void Cancel() {
            IsCancelled = true;
        }

        public bool IsCancelled { get; private set; }
    }
}