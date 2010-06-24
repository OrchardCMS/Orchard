using Orchard.ContentManagement.MetaData.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class AddFieldViewModel : BaseViewModel {
        public AddFieldViewModel(ContentPartDefinition part) {
            Part = part;
        }

        public ContentPartDefinition Part { get; private set; }
    }
}