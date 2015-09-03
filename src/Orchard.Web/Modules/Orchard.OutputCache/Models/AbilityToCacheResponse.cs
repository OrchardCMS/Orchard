namespace Orchard.OutputCache.Models
{
    public class AbilityToCacheResponse {

        public AbilityToCacheResponse() {
            
        }

        public AbilityToCacheResponse(bool canBeCached, string message) {
            CanBeCached = canBeCached;
            Message = message;
        }

        public bool CanBeCached { get; set; }
        public string Message { get; set; }
    }
}