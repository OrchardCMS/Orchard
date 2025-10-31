using Orchard.MediaLibrary.WebSearch.Providers;

namespace Orchard.MediaLibrary.WebSearch
{
    public static class WebSearchProviderExtensions
    {
        public static string GetApiKey(this IWebSearchProvider provider) => provider.Settings.ApiKey;

        public static bool IsValid(this IWebSearchProvider provider) => !string.IsNullOrEmpty(provider.GetApiKey());
    }
}