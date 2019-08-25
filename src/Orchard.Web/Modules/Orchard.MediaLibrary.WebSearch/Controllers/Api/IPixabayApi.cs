using System.Threading.Tasks;
using RestEase;

namespace Orchard.MediaLibrary.WebSearch.Controllers.Api {
    [Header("User-Agent", "RestEase")]
    public interface IPixabayApi {
        [Get("api/")]
        Task<string> GetImagesAsync(string key, string q);
    }
}