using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Core.Containers.ViewModels {
    public class ContainerViewModel {
        public IList<string> SelectedItemContentTypes { get; set; }
        public bool ItemsShown { get; set; }
        public bool Paginated { get; set; }
        public int PageSize { get; set; }
        public bool ShowOnAdminMenu { get; set; }
        public string AdminMenuText { get; set; }
        public string AdminMenuPosition { get; set; }
        public string AdminMenuImageSet { get; set; }
        public IList<ContentTypeDefinition> AvailableItemContentTypes { get; set; }
        public bool RestrictItemContentTypes { get; set; }
        public bool EnablePositioning { get; set; }
        public bool OverrideEnablePositioning { get; set; }
    }

    public enum SortBy {
        Modified,
        Published,
        Created,
        DisplayText
    }

    public enum SortDirection {
        Ascending,
        Descending
    }
}