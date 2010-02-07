using System.Collections.Generic;
using Orchard.Mvc.ViewModels;
using Orchard.Tags.Models;

namespace Orchard.Tags.ViewModels {
    public class TagsIndexViewModel : BaseViewModel {
        public IList<Tag> Tags { get; set; }
    }
}
