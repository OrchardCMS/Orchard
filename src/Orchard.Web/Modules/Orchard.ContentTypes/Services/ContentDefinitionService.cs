using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.ViewModels;
using Orchard.Localization;

namespace Orchard.ContentTypes.Services {
    public class ContentDefinitionService : IContentDefinitionService {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IContentDefinitionEditorEvents _contentDefinitionEditorEvents;

        public ContentDefinitionService(
                IOrchardServices services,
                IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentFieldDriver> contentFieldDrivers,
                IContentDefinitionEditorEvents contentDefinitionEditorEvents) {
            Services = services;
            _contentDefinitionManager = contentDefinitionManager;
            _contentFieldDrivers = contentFieldDrivers;
            _contentDefinitionEditorEvents = contentDefinitionEditorEvents;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<EditTypeViewModel> GetTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Select(ctd => new EditTypeViewModel(ctd));
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

        public EditTypeViewModel AddType(CreateTypeViewModel typeViewModel) {
            var name = GenerateName(typeViewModel.DisplayName);

            while (_contentDefinitionManager.GetTypeDefinition(name) != null)
                name = VersionName(name);

            var contentTypeDefinition = new ContentTypeDefinition(name, typeViewModel.DisplayName);
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);

            return new EditTypeViewModel(contentTypeDefinition);
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

        public IEnumerable<EditPartViewModel> GetParts() {
            var typeNames = GetTypes().Select(ctd => ctd.Name);
            return _contentDefinitionManager.ListPartDefinitions().Where(cpd => !typeNames.Contains(cpd.Name)).Select(cpd => new EditPartViewModel(cpd));
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
            var name = GenerateName(partViewModel.Name);

            while (_contentDefinitionManager.GetPartDefinition(name) != null)
                name = VersionName(name);

            var contentPartDefinition = new ContentPartDefinition(name);
            _contentDefinitionManager.StorePartDefinition(contentPartDefinition);

            return new EditPartViewModel(contentPartDefinition);
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
            _contentDefinitionManager.AlterPartDefinition(partName, partBuilder => 
                partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName))
            );
        }

        public void RemoveFieldFromPart(string fieldName, string partName) {
            _contentDefinitionManager.AlterPartDefinition(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
        }

        //gratuitously stolen from the RoutableService
        private static string GenerateName(string displayName) {
            if (string.IsNullOrWhiteSpace(displayName))
                return "";

            var name = displayName;
            //todo: might need to be made more restrictive depending on how name is used (like as an XML node name, for instance)
            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s]+");

            name = dissallowed.Replace(name, "-");
            name = name.Trim('-');

            if (name.Length > 128)
                name = name.Substring(0, 128);

            return name.ToLowerInvariant();
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