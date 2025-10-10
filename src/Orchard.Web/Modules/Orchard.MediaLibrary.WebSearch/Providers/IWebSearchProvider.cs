using System.Collections.Generic;
using Orchard.MediaLibrary.WebSearch.Models;
using Orchard.MediaLibrary.WebSearch.ViewModels;

namespace Orchard.MediaLibrary.WebSearch.Providers {
    public interface IWebSearchProvider : IDependency {
        string Name { get; }
        IWebSearchSettings Settings { get; }
        IEnumerable<WebSearchResult> GetImages(string query);
    }
}