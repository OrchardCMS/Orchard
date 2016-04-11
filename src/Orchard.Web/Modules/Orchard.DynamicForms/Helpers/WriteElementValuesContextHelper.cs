using System.Collections.Generic;
using Orchard.DynamicForms.Services;

namespace Orchard.Layouts.Helpers {
    public static class WriteElementValuesContextHelper {
        
        public static IDictionary<string, object> GetTokenData(this WriteElementValuesContext context) {
            var data = new Dictionary<string, object>();

            if (context.Content != null)
                data["Content"] = context.Content.ContentItem;

            return data;
        }
    }
}