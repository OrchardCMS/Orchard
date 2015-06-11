namespace Orchard.Warmup.Services {
    public interface IWarmupUpdater : IDependency {
        /// <summary>
        ///  Forces a regeneration of all static pages
        /// </summary>
        void Generate();

        /// <summary>
        /// Generates static pages if needed
        /// </summary>
        void EnsureGenerate();
    }
}
