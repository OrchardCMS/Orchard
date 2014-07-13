namespace Orchard.Azure.MediaServices.Services.Tasks {

    /// <summary>
    /// Represents a specific configuration of a task provider for an individual task instance.
    /// </summary>
    public class TaskConfiguration {

        public TaskConfiguration(ITaskProvider provider) {
            TaskProvider = provider;
        }

        /// <summary>
        /// The provider that created this configuration
        /// </summary>
        public ITaskProvider TaskProvider { get; private set; }

        public dynamic EditorShape { get; set; }

        /// <summary>
        /// Used by the task provider to store the configuration information required to create an ITask instance.
        /// </summary>
        public dynamic Settings { get; set; }
    }
}