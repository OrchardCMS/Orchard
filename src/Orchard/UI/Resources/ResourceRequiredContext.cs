using System;
using System.Web.Mvc;

namespace Orchard.UI.Resources {
    public class ResourceRequiredContext {
        public ResourceDefinition Resource { get; set; }
        public RequireSettings Settings { get; set; }

        public TagBuilder GetTagBuilder(RequireSettings baseSettings, string appPath) {
            var tagBuilder = new TagBuilder(Resource.TagName);
            tagBuilder.MergeAttributes(Resource.TagBuilder.Attributes);
            if (!String.IsNullOrEmpty(Resource.FilePathAttributeName)) {
                var resolvedUrl = Resource.ResolveUrl(baseSettings == null ? Settings : baseSettings.Combine(Settings), appPath);
                if (!String.IsNullOrEmpty(resolvedUrl)) {
                    tagBuilder.MergeAttribute(Resource.FilePathAttributeName, resolvedUrl, true);
                }
            }
            return tagBuilder;
        }
    }
}
