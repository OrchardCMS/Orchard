using System.Threading.Tasks;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Controllers.Api {
    [Header("User-Agent", "RestEase")]
    public interface IGoogleApi {
        [Get("customsearch/v1")]
        Task<string> GetImagesAsync(string key, string cx, string q, string searchType = "image");
    }
}