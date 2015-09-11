namespace Orchard.Environment {
    public class StandaloneEnvironmentOptions {
        public static readonly StandaloneEnvironmentOptions None = new StandaloneEnvironmentOptions();
        public static readonly StandaloneEnvironmentOptions RunningEnvironment = new StandaloneEnvironmentOptions {Running = true};
        public bool Running { get; set; }
    }
}