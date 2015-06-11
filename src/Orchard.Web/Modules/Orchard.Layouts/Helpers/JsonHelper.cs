using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.Layouts.Helpers {
    public static class JsonHelper {
        
        public static string ToJson(this object value, Formatting formatting = Formatting.None) {
            return value != null ? JObject.FromObject(value).ToString(formatting) : default(string);
        }
    }
}