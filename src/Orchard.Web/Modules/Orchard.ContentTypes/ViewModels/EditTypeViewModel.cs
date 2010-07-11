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
            Fields = GetTypeFields(contentTypeDefinition).ToList();
            Parts = GetTypeParts(contentTypeDefinition).ToList();
            _Definition = contentTypeDefinition;
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public SettingsDictionary Settings { get; set; }
        public IEnumerable<EditPartFieldViewModel> Fields { get; set; }
        public IEnumerable<EditTypePartViewModel> Parts { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }
        public ContentTypeDefinition _Definition { get; private set; }

        private IEnumerable<EditPartFieldViewModel> GetTypeFields(ContentTypeDefinition contentTypeDefinition) {
            var implicitTypePart = contentTypeDefinition.Parts.SingleOrDefault(p => p.PartDefinition.Name == Name);

            return implicitTypePart == null
                ? Enumerable.Empty<EditPartFieldViewModel>()
                : implicitTypePart.PartDefinition.Fields.Select((f, i) => new EditPartFieldViewModel(i, f) { Part = new EditPartViewModel(implicitTypePart.PartDefinition) });
        }

        private IEnumerable<EditTypePartViewModel> GetTypeParts(ContentTypeDefinition contentTypeDefinition) {
            return contentTypeDefinition.Parts
                .Where(p => p.PartDefinition.Name != Name)
                .Select((p, i) => new EditTypePartViewModel(i, p) { Type = this });
        }
    }

    public class EditTypePartViewModel {
        public EditTypePartViewModel() {
            Settings = new SettingsDictionary();
        }

        public EditTypePartViewModel(int index, ContentTypeDefinition.Part part) {
            Index = index;
            PartDefinition = new EditPartViewModel(part.PartDefinition);
            Settings = part.Settings;
            _Definition = part;
        }

        public int Index { get; set; }
        public string Prefix { get { return "Parts[" + Index + "]"; } }
        public EditPartViewModel PartDefinition { get; set; }
        public SettingsDictionary Settings { get; set; }
        public EditTypeViewModel Type { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }
        public ContentTypeDefinition.Part _Definition { get; private set; }
    }

    public class EditPartViewModel : BaseViewModel {
        public EditPartViewModel() {
            Fields = new List<EditPartFieldViewModel>();
            Settings = new SettingsDictionary();
        }

        public EditPartViewModel(ContentPartDefinition contentPartDefinition) {
            Name = contentPartDefinition.Name;
            Fields = contentPartDefinition.Fields.Select((f, i) => new EditPartFieldViewModel(i, f) { Part = this }).ToList();
            Settings = contentPartDefinition.Settings;
            _Definition = contentPartDefinition;
        }

        public string Prefix { get { return "PartDefinition"; } }
        public string Name { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }
        public IEnumerable<EditPartFieldViewModel> Fields { get; set; }
        public SettingsDictionary Settings { get; set; }
        public ContentPartDefinition _Definition { get; private set; }
    }

    public class EditPartFieldViewModel {

        public EditPartFieldViewModel() {
            Settings = new SettingsDictionary();
        }

        public EditPartFieldViewModel(int index, ContentPartDefinition.Field field) {
            Index = index;
            Name = field.Name;
            FieldDefinition = new EditFieldViewModel(field.FieldDefinition);
            Settings = field.Settings;
            _Definition = field;
        }

        public int Index { get; set; }
        public string Prefix { get { return "Fields[" + Index + "]"; } }
        public EditPartViewModel Part { get; set; }

        public string Name { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }
        public EditFieldViewModel FieldDefinition { get; set; }
        public SettingsDictionary Settings { get; set; }
        public ContentPartDefinition.Field _Definition { get; private set; }
    }

    public class EditFieldViewModel {
        public EditFieldViewModel() { }

        public EditFieldViewModel(ContentFieldDefinition contentFieldDefinition) {
            Name = contentFieldDefinition.Name;
            _Definition = contentFieldDefinition;
        }

        public string Name { get; set; }
        public ContentFieldDefinition _Definition { get; private set; }
    }
}
