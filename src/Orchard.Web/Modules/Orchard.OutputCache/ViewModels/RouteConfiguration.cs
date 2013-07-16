namespace Orchard.OutputCache.ViewModels {
    public class RouteConfiguration {
        public string RouteKey { get; set; }
        public string Url { get; set; }
        public int Priority { get; set; }
        public int? Duration { get; set; }
        public int? MaxAge { get; set; }
        public string FeatureName { get; set; }
    }
}