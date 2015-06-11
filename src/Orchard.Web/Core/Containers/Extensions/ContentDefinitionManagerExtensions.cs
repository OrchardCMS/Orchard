using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Core.Containers.Extensions {
    public static class ContentDefinitionManagerExtensions {
        public static IEnumerable<ContentTypeDefinition> ParseContentTypeDefinitions(this IContentDefinitionManager contentDefinitionManager, string contentTypes) {
            if (contentTypes == null)
                return Enumerable.Empty<ContentTypeDefinition>();

            var typeNames = contentTypes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var typeDefinitions = contentDefinitionManager.ListTypeDefinitions().ToDictionary(x => x.Name, x => x);
            return typeNames.Select(x => x.Trim()).Where(typeDefinitions.ContainsKey).Select(x => typeDefinitions[x]);
        }

        public static string JoinContentTypeDefinitions(this IContentDefinitionManager contentDefinitionManager, IEnumerable<ContentTypeDefinition> contentTypes) {
            return contentTypes == null ? null : String.Join(",", contentTypes.Select(x => x.Name));
        }
    }
}