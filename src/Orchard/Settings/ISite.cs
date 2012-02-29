using System;
using Orchard.ContentManagement;

namespace Orchard.Settings {
    /// <summary>
    /// Interface provided by the "settings" model.
    /// </summary>
    public interface ISite : IContent {
        string PageTitleSeparator { get; }
        string SiteName { get; }
        string SiteSalt { get; }
        string SuperUser { get; }
        string HomePage { get; set; }
        string SiteCulture { get; set; }
        ResourceDebugMode ResourceDebugMode { get; set; }
        int PageSize { get; set; }
        string BaseUrl { get; }
        string SiteTimeZone { get; }
    }
}
