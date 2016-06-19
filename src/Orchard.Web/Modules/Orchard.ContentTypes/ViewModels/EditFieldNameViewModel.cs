using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentTypes.ViewModels {
    public class EditFieldNameViewModel {
        public EditFieldNameViewModel(ContentPartDefinition partDefinition, ContentTypeDefinition typeDefinition) {
            PartDefinition = partDefinition;
            TypeDefinition = typeDefinition;
        }

        /// <summary>
        /// The technical name of the field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The display name of the field
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        public ContentPartDefinition PartDefinition { get; private set; }
        public ContentTypeDefinition TypeDefinition { get; private set; }
    }
}