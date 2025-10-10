using System.Threading.Tasks;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Controllers.Api {
    [Header("User-Agent", "RestEase")]
    public interface IBingApi {
        [Get("bing/v7.0/images/search")]
        Task<string> GetImagesAsync([Header("Ocp-Apim-Subscription-Key")] string apiKey, string q);
    }
}