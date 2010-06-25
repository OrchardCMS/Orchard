using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;

namespace Orchard.ContentTypes.ViewModels {
    public class EditTypeViewModel : BaseViewModel {
        public EditTypeViewModel() {
            Settings = new SettingsDictionary();
            Fields = new List<EditPartFieldViewModel>();
            Parts = new List<EditTypePartViewModel>();
        }
        public EditTypeViewModel(ContentTypeDefinition contentTypeDefinition) {
            Name = contentTypeDefinition.Name;
            DisplayName = contentTypeDefinition.DisplayName;
            Settings = contentTypeDefinition.Settings;
            Fields = GetTypeFields(contentTypeDefinition);
            Parts = GetTypeParts(contentTypeDefinition);
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }

        public SettingsDictionary Settings { get; set; }
        public IEnumerable<EditPartFieldViewModel> Fields { get; set; }
        public IEnumerable<EditTypePartViewModel> Parts { get; set; }

        private IEnumerable<EditPartFieldViewModel> GetTypeFields(ContentTypeDefinition contentTypeDefinition) {
            var implicitTypePart = contentTypeDefinition.Parts.SingleOrDefault(p => p.PartDefinition.Name == Name);

            return implicitTypePart == null
                ? Enumerable.Empty<EditPartFieldViewModel>()
                : implicitTypePart.PartDefinition.Fields.Select(f => new EditPartFieldViewModel(f));
        }

        private IEnumerable<EditTypePartViewModel> GetTypeParts(ContentTypeDefinition contentTypeDefinition) {
            return contentTypeDefinition.Parts.Where(p => p.PartDefinition.Name != Name).Select(p => new EditTypePartViewModel(p));
        }
    }

    public class EditTypePartViewModel {
        public EditTypePartViewModel() {
            Settings = new SettingsDictionary();
        }
        public EditTypePartViewModel(ContentTypeDefinition.Part part) {
            PartDefinition = new EditPartViewModel(part.PartDefinition);
            Settings = part.Settings;
        }

        public EditPartViewModel PartDefinition { get; set; }
        public SettingsDictionary Settings { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }
    }

    public class EditPartViewModel : BaseViewModel {
        public EditPartViewModel() {
            Fields = new List<EditPartFieldViewModel>();
            Settings = new SettingsDictionary();
        }
        public EditPartViewModel(ContentPartDefinition contentPartDefinition) {
            Name = contentPartDefinition.Name;
            Fields = contentPartDefinition.Fields.Select(f => new EditPartFieldViewModel(f));
            Settings = contentPartDefinition.Settings;
        }

        public string Name { get; set; }
        public IEnumerable<EditPartFieldViewModel> Fields { get; set; }
        public SettingsDictionary Settings { get; set; }
    }

    public class EditPartFieldViewModel {
        public EditPartFieldViewModel() {
            Settings = new SettingsDictionary();
        }
        public EditPartFieldViewModel(ContentPartDefinition.Field field) {
            Name = field.Name;
            FieldDefinition = new EditFieldViewModel(field.FieldDefinition);
            Settings = field.Settings;
        }

        public string Name { get; set; }
        public EditFieldViewModel FieldDefinition { get; set; }
        public SettingsDictionary Settings { get; set; }
    }

    public class EditFieldViewModel {
        public EditFieldViewModel() { }
        public EditFieldViewModel(ContentFieldDefinition contentFieldDefinition) {
            Name = contentFieldDefinition.Name;
        }

        public string Name { get; set; }
    }
}
