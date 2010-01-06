using System;
using System.Web.Mvc;
using Orchard.Pages.Models;

namespace Orchard.Pages.Services.Templates {
    public static class TemplateExtensions {
        /// <summary>
        /// Include a zone's contents from the current view.
        /// </summary>
        public static string IncludeZone(this HtmlHelper helper, string zoneName) {
            //TODO: Validation
            if (String.IsNullOrEmpty(zoneName)) {
                throw new ArgumentNullException("zoneName");
            }
            PageRevision revision = helper.ViewData.Model as PageRevision;
            if (revision != null) {
                foreach (ContentItem contentItem in revision.Contents) {
                    if (contentItem.ZoneName.Equals(zoneName, StringComparison.OrdinalIgnoreCase)) {
                        return contentItem.Content;
                    }
                }
            }
            return String.Empty;
        }
    }
}