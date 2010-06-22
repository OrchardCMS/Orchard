using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class EditTypeViewModel : BaseViewModel {
        public EditTypeViewModel() {
            Settings = new SettingsDictionary();
            Parts = new List<EditTypePartViewModel>();
        }
        public EditTypeViewModel(ContentTypeDefinition contentTypeDefinition) {
            Name = contentTypeDefinition.Name;
            DisplayName = contentTypeDefinition.DisplayName;
            Settings = contentTypeDefinition.Settings;
            Parts = contentTypeDefinition.Parts.Select(p => new EditTypePartViewModel(p));
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public SettingsDictionary Settings { get; set; }
        public IEnumerable<EditTypePartViewModel> Parts { get; set; }
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
            Name = Name;
        }

        public string Name { get; set; }
    }
}
