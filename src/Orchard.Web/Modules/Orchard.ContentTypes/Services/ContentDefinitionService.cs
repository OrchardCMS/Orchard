using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.ViewModels;
using Orchard.Core.Contents.Extensions;
using Orchard.Localization;

namespace Orchard.ContentTypes.Services {
    public class ContentDefinitionService : IContentDefinitionService {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IContentDefinitionEditorEvents _contentDefinitionEditorEvents;

        public ContentDefinitionService(
                IOrchardServices services,
                IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentPartDriver> contentPartDrivers,
                IEnumerable<IContentFieldDriver> contentFieldDrivers,
                IContentDefinitionEditorEvents contentDefinitionEditorEvents)
        {
            Services = services;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _contentDefinitionEditorEvents = contentDefinitionEditorEvents;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<EditTypeViewModel> GetTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Select(ctd => new EditTypeViewModel(ctd)).OrderBy(m => m.DisplayName);
        }

        public EditTypeViewModel GetType(string name) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(name);

            if (contentTypeDefinition == null)
                return null;

            var viewModel = new EditTypeViewModel(contentTypeDefinition) {
                Templates = _contentDefinitionEditorEvents.TypeEditor(contentTypeDefinition)
            };

            foreach (var part in viewModel.Parts) {
                part.Templates = _contentDefinitionEditorEvents.TypePartEditor(part._Definition);
                foreach (var field in part.PartDefinition.Fields)
                    field.Templates = _contentDefinitionEditorEvents.PartFieldEditor(field._Definition);
            }

            if (viewModel.Fields.Any()) {
                foreach (var field in viewModel.Fields)
                    field.Templates = _contentDefinitionEditorEvents.PartFieldEditor(field._Definition);
            }

            return viewModel;
        }

        public ContentTypeDefinition AddType(string name, string displayName) {
            if(String.IsNullOrWhiteSpace(displayName)) {
                throw new ArgumentException("displayName");
            }

            if(String.IsNullOrWhiteSpace(name)) {
                name = GenerateContentTypeNameFromDisplayName(displayName);
            }
            else {
                if(!IsLetter(name[0])) {
                    throw new ArgumentException("Content type name must start with a letter", "name");
                }
            }

            while ( _contentDefinitionManager.GetTypeDefinition(name) != null )
                name = VersionName(name);

            var contentTypeDefinition = new ContentTypeDefinition(name, displayName);
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
            _contentDefinitionManager.AlterTypeDefinition(name,
                cfg => cfg.Creatable().Draftable());

            return contentTypeDefinition;
        }

        public void AlterType(EditTypeViewModel typeViewModel, IUpdateModel updateModel) {
            var updater = new Updater(updateModel);
            _contentDefinitionManager.AlterTypeDefinition(typeViewModel.Name, typeBuilder => {
                typeBuilder.DisplayedAs(typeViewModel.DisplayName);

                // allow extensions to alter type configuration
                typeViewModel.Templates = _contentDefinitionEditorEvents.TypeEditorUpdate(typeBuilder, updater);

                foreach (var part in typeViewModel.Parts) {
                    var partViewModel = part;

                    // enable updater to be aware of changing part prefix
                    updater._prefix = secondHalf => string.Format("{0}.{1}", partViewModel.Prefix, secondHalf);

                    // allow extensions to alter typePart configuration
                    typeBuilder.WithPart(partViewModel.PartDefinition.Name, typePartBuilder => {
                        partViewModel.Templates = _contentDefinitionEditorEvents.TypePartEditorUpdate(typePartBuilder, updater);
                    });

                    if (!partViewModel.PartDefinition.Fields.Any())
                        continue;

                    _contentDefinitionManager.AlterPartDefinition(partViewModel.PartDefinition.Name, partBuilder => {
                        var fieldFirstHalf = string.Format("{0}.{1}", partViewModel.Prefix, partViewModel.PartDefinition.Prefix);
                        foreach (var field in partViewModel.PartDefinition.Fields) {
                            var fieldViewModel = field;

                            // enable updater to be aware of changing field prefix
                            updater._prefix = secondHalf =>
                                string.Format("{0}.{1}.{2}", fieldFirstHalf, fieldViewModel.Prefix, secondHalf);
                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder => {
                                fieldViewModel.Templates = _contentDefinitionEditorEvents.PartFieldEditorUpdate(partFieldBuilder, updater);
                            });
                        }
                    });
                }

                if (typeViewModel.Fields.Any()) {
                    _contentDefinitionManager.AlterPartDefinition(typeViewModel.Name, partBuilder => {
                        foreach (var field in typeViewModel.Fields) {
                            var fieldViewModel = field;

                            // enable updater to be aware of changing field prefix
                            updater._prefix = secondHalf =>
                                string.Format("{0}.{1}", fieldViewModel.Prefix, secondHalf);

                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder => {
                                fieldViewModel.Templates = _contentDefinitionEditorEvents.PartFieldEditorUpdate(partFieldBuilder, updater);
                            });
                        }
                    });
                }
            });
        }

        public void RemoveType(string name) {
            throw new NotImplementedException();
        }

        public void AddPartToType(string partName, string typeName) {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.WithPart(partName));
        }

        public void RemovePartFromType(string partName, string typeName) {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.RemovePart(partName));
        }

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly) {
            var typeNames = GetTypes().Select(ctd => ctd.Name);

            // user-defined parts
            // except for those parts with the same name as a type (implicit type's part or a mistake)
            var userContentParts = _contentDefinitionManager
                .ListPartDefinitions()
                .Where(cpd => !typeNames.Contains(cpd.Name))
                .Select(cpd => new EditPartViewModel(cpd));

            // code-defined parts
            var codeDefinedParts = metadataPartsOnly ? 
                Enumerable.Empty<EditPartViewModel>() : 
                _contentPartDrivers.SelectMany(d => d.GetPartInfo().Where(cpd => !userContentParts.Any(m => m.Name == cpd.PartName)).Select(cpi => new EditPartViewModel { Name = cpi.PartName }));

            // Order by display name
            return userContentParts.Union(codeDefinedParts).OrderBy(m => m.DisplayName);
        }

        public EditPartViewModel GetPart(string name) {
            var contentPartDefinition = _contentDefinitionManager.GetPartDefinition(name);

            if (contentPartDefinition == null)
                return null;

            var viewModel = new EditPartViewModel(contentPartDefinition) {
                Templates = _contentDefinitionEditorEvents.PartEditor(contentPartDefinition)
            };

            return viewModel;
        }

        public EditPartViewModel AddPart(CreatePartViewModel partViewModel) {
            var name = partViewModel.Name;

            if (_contentDefinitionManager.GetPartDefinition(name) != null)
                throw new OrchardException(T("Cannot add part named '{0}'. It already exists.", name));

            if (!String.IsNullOrEmpty(name)) {
                _contentDefinitionManager.AlterPartDefinition(name, builder => builder.Attachable());
                return new EditPartViewModel(_contentDefinitionManager.GetPartDefinition(name));
            }

            return null;
        }

        public void AlterPart(EditPartViewModel partViewModel, IUpdateModel updateModel) {
            var updater = new Updater(updateModel);
            _contentDefinitionManager.AlterPartDefinition(partViewModel.Name, partBuilder => {
                partViewModel.Templates = _contentDefinitionEditorEvents.PartEditorUpdate(partBuilder, updater);
            });
        }

        public void RemovePart(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentFieldInfo> GetFields() {
            return _contentFieldDrivers.SelectMany(d => d.GetFieldInfo());
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName) {
            fieldName = SafeName(fieldName);
            if (string.IsNullOrEmpty(fieldName)) {
                throw new OrchardException(T("Fields must have a name containing no spaces or symbols."));
            }
            _contentDefinitionManager.AlterPartDefinition(partName,
                partBuilder => partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName)));
        }

        public void RemoveFieldFromPart(string fieldName, string partName) {
            _contentDefinitionManager.AlterPartDefinition(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
        }

        private static string SafeName(string name) {
            if (string.IsNullOrWhiteSpace(name))
                return String.Empty;

            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s\""<>]+");

            name = dissallowed.Replace(name, String.Empty);
            name = name.Trim();

            // don't allow non A-Z chars as first letter, as they are not allowed in prefixes
            if(name.Length > 0) {
                if (!IsLetter(name[0])) {
                    name = name.Substring(1);
                }
            }

            if (name.Length > 128)
                name = name.Substring(0, 128);

            return name;
        }

        public static bool IsLetter(char c) {
            return ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z');
        }

        //gratuitously stolen from the RoutableService
        public string GenerateContentTypeNameFromDisplayName(string displayName) {
            displayName = SafeName(displayName);

            while ( _contentDefinitionManager.GetTypeDefinition(displayName) != null )
                displayName = VersionName(displayName);

            return displayName;
        }

        private static string VersionName(string name) {
            int version;
            var nameParts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1 && int.TryParse(nameParts.Last(), out version)) {
                version = version > 0 ? ++version : 2;
                //this could unintentionally chomp something that looks like a version
                name = string.Join("-", nameParts.Take(nameParts.Length - 1));
            }
            else {
                version = 2;
            }

            return string.Format("{0}-{1}", name, version);
        }

        class Updater : IUpdateModel {
            private readonly IUpdateModel _thunk;

            public Updater(IUpdateModel thunk) {
                _thunk = thunk;
            }

            public Func<string, string> _prefix = x => x;

            public bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class {
                return _thunk.TryUpdateModel(model, _prefix(prefix), includeProperties, excludeProperties);
            }

            public void AddModelError(string key, LocalizedString errorMessage) {
                _thunk.AddModelError(_prefix(key), errorMessage);
            }
        }
    }
}