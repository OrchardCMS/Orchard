using System.Collections.Generic;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerChildFoldersViewModel {
        public IEnumerable<IMediaFolder> Children { get; set; }
    }
}
