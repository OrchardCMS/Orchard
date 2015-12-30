﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Events;
using Orchard.ContentTypes.ViewModels;
using Orchard.Core.Contents.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Utility.Extensions;

namespace Orchard.ContentTypes.Services {
    public class ContentDefinitionService : IContentDefinitionService {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IContentDefinitionEditorEvents _contentDefinitionEditorEvents;
        private readonly IContentDefinitionEventHandler _contentDefinitionEventHandlers;

        public ContentDefinitionService(
                IOrchardServices services,
                IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentPartDriver> contentPartDrivers,
                IEnumerable<IContentFieldDriver> contentFieldDrivers,
                IContentDefinitionEditorEvents contentDefinitionEditorEvents,
                IContentDefinitionEventHandler contentDefinitionEventHandlers) {

            Services = services;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _contentDefinitionEditorEvents = contentDefinitionEditorEvents;
            _contentDefinitionEventHandlers = contentDefinitionEventHandlers;
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
                part._Definition.ContentTypeDefinition = contentTypeDefinition;
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
                if(!name[0].IsLetter()) {
                    throw new ArgumentException("Content type name must start with a letter", "name");
                }
            }

            while ( _contentDefinitionManager.GetTypeDefinition(name) != null )
                name = VersionName(name);

            var contentTypeDefinition = new ContentTypeDefinition(name, displayName);
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
            _contentDefinitionManager.AlterTypeDefinition(name, cfg => cfg.Creatable().Draftable().Listable().Securable());
            _contentDefinitionEventHandlers.ContentTypeCreated(new ContentTypeCreatedContext { ContentTypeDefinition = contentTypeDefinition});

            return contentTypeDefinition;
        }

        public void AlterType(EditTypeViewModel typeViewModel, IUpdateModel updateModel) {
            var updater = new Updater(updateModel);
            _contentDefinitionManager.AlterTypeDefinition(typeViewModel.Name, typeBuilder => {
                typeBuilder.DisplayedAs(typeViewModel.DisplayName);

                // allow extensions to alter type configuration
                _contentDefinitionEditorEvents.TypeEditorUpdating(typeBuilder);
                typeViewModel.Templates = _contentDefinitionEditorEvents.TypeEditorUpdate(typeBuilder, updater);
                _contentDefinitionEditorEvents.TypeEditorUpdated(typeBuilder);

                foreach (var part in typeViewModel.Parts) {
                    var partViewModel = part;

                    // enable updater to be aware of changing part prefix
                    updater.Prefix = secondHalf => String.Format("{0}.{1}", partViewModel.Prefix, secondHalf);

                    // allow extensions to alter typePart configuration
                    typeBuilder.WithPart(partViewModel.PartDefinition.Name, typePartBuilder => {
                        _contentDefinitionEditorEvents.TypePartEditorUpdating(typePartBuilder);
                        partViewModel.Templates = _contentDefinitionEditorEvents.TypePartEditorUpdate(typePartBuilder, updater);
                        _contentDefinitionEditorEvents.TypePartEditorUpdated(typePartBuilder);
                    });

                    if (!partViewModel.PartDefinition.Fields.Any())
                        continue;

                    _contentDefinitionManager.AlterPartDefinition(partViewModel.PartDefinition.Name, partBuilder => {
                        var fieldFirstHalf = String.Format("{0}.{1}", partViewModel.Prefix, partViewModel.PartDefinition.Prefix);
                        foreach (var field in partViewModel.PartDefinition.Fields) {
                            var fieldViewModel = field;

                            // enable updater to be aware of changing field prefix
                            updater.Prefix = secondHalf =>
                                String.Format("{0}.{1}.{2}", fieldFirstHalf, fieldViewModel.Prefix, secondHalf);
                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder => {
                                _contentDefinitionEditorEvents.PartFieldEditorUpdating(partFieldBuilder);
                                fieldViewModel.Templates = _contentDefinitionEditorEvents.PartFieldEditorUpdate(partFieldBuilder, updater);
                                _contentDefinitionEditorEvents.PartFieldEditorUpdated(partFieldBuilder);
                            });
                        }
                    });
                }

                if (typeViewModel.Fields.Any()) {
                    _contentDefinitionManager.AlterPartDefinition(typeViewModel.Name, partBuilder => {
                        foreach (var field in typeViewModel.Fields) {
                            var fieldViewModel = field;

                            // enable updater to be aware of changing field prefix
                            updater.Prefix = secondHalf =>
                                string.Format("{0}.{1}", fieldViewModel.Prefix, secondHalf);

                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder => {
                                _contentDefinitionEditorEvents.PartFieldEditorUpdating(partFieldBuilder);
                                fieldViewModel.Templates = _contentDefinitionEditorEvents.PartFieldEditorUpdate(partFieldBuilder, updater);
                                _contentDefinitionEditorEvents.PartFieldEditorUpdated(partFieldBuilder);
                            });
                        }
                    });
                }
            });
        }

        public void RemoveType(string name, bool deleteContent) {

            // first remove all attached parts
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(name);
            var partDefinitions = typeDefinition.Parts.ToArray();
            foreach (var partDefinition in partDefinitions) {
                RemovePartFromType(partDefinition.PartDefinition.Name, name);

                // delete the part if it's its own part
                if(partDefinition.PartDefinition.Name == name) {
                    RemovePart(name);
                }
            }

            _contentDefinitionManager.DeleteTypeDefinition(name);

            // delete all content items (but keep versions)
            if (deleteContent) {
                var contentItems = Services.ContentManager.Query(name).List();
                foreach (var contentItem in contentItems) {
                    Services.ContentManager.Remove(contentItem);
                }
            }
            _contentDefinitionEventHandlers.ContentTypeRemoved(new ContentTypeRemovedContext { ContentTypeDefinition = typeDefinition });
        }

        public void AddPartToType(string partName, string typeName) {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.WithPart(partName));
            _contentDefinitionEventHandlers.ContentPartAttached(new ContentPartAttachedContext { ContentTypeName = typeName, ContentPartName = partName});
        }

        public void RemovePartFromType(string partName, string typeName) {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.RemovePart(partName));
            _contentDefinitionEventHandlers.ContentPartDetached(new ContentPartDetachedContext {ContentTypeName = typeName, ContentPartName = partName});
        }

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly) {
            var typeNames = new HashSet<string>(GetTypes().Select(ctd => ctd.Name));

            // user-defined parts
            // except for those parts with the same name as a type (implicit type's part or a mistake)
            var userContentParts = _contentDefinitionManager.ListPartDefinitions()
                .Where(cpd => !typeNames.Contains(cpd.Name))
                .Select(cpd => new EditPartViewModel(cpd))
                .ToDictionary(
                    k => k.Name,
                    v => v);

            // code-defined parts
            var codeDefinedParts = metadataPartsOnly
                ? Enumerable.Empty<EditPartViewModel>()
                : _contentPartDrivers
                    .SelectMany(d => d.GetPartInfo()
                        .Where(cpd => !userContentParts.ContainsKey(cpd.PartName))
                        .Select(cpi => new EditPartViewModel { Name = cpi.PartName, DisplayName = cpi.PartName }))
                    .ToList();

            // Order by display name
            return codeDefinedParts
                .Union(userContentParts.Values)
                .OrderBy(m => m.DisplayName);
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
                var partDefinition = _contentDefinitionManager.GetPartDefinition(name);
                _contentDefinitionEventHandlers.ContentPartCreated(new ContentPartCreatedContext {ContentPartDefinition = partDefinition});
                return new EditPartViewModel(partDefinition);
            }

            return null;
        }

        public void AlterPart(EditPartViewModel partViewModel, IUpdateModel updateModel) {
            var updater = new Updater(updateModel);
            _contentDefinitionManager.AlterPartDefinition(partViewModel.Name, partBuilder => {
                _contentDefinitionEditorEvents.PartEditorUpdating(partBuilder);
                partViewModel.Templates = _contentDefinitionEditorEvents.PartEditorUpdate(partBuilder, updater);
                _contentDefinitionEditorEvents.PartEditorUpdated(partBuilder);
            });
        }

        public void RemovePart(string name) {
            var partDefinition = _contentDefinitionManager.GetPartDefinition(name);
            var fieldDefinitions = partDefinition.Fields.ToArray();
            foreach (var fieldDefinition in fieldDefinitions) {
                RemoveFieldFromPart(fieldDefinition.Name, name);
            }

            _contentDefinitionManager.DeletePartDefinition(name);
            _contentDefinitionEventHandlers.ContentPartRemoved(new ContentPartRemovedContext {ContentPartDefinition = partDefinition});
        }

        public IEnumerable<ContentFieldInfo> GetFields() {
            return _contentFieldDrivers.SelectMany(d => d.GetFieldInfo());
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName) {
            AddFieldToPart(fieldName, fieldName, fieldTypeName, partName);
        }

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName) {
            fieldName = fieldName.ToSafeName();
            if (string.IsNullOrEmpty(fieldName)) {
                throw new OrchardException(T("Fields must have a name containing no spaces or symbols."));
            }
            _contentDefinitionManager.AlterPartDefinition(partName,
                partBuilder => partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName).WithDisplayName(displayName)));

            _contentDefinitionEventHandlers.ContentFieldAttached(new ContentFieldAttachedContext {
                ContentPartName = partName,
                ContentFieldTypeName = fieldTypeName,
                ContentFieldName = fieldName,
                ContentFieldDisplayName = displayName
            });
        }

        public void RemoveFieldFromPart(string fieldName, string partName) {
            _contentDefinitionManager.AlterPartDefinition(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
            _contentDefinitionEventHandlers.ContentFieldDetached(new ContentFieldDetachedContext {
                ContentPartName = partName,
                ContentFieldName = fieldName
            });
        }

        public void AlterField(EditPartViewModel partViewModel, EditFieldNameViewModel fieldViewModel) {
            _contentDefinitionManager.AlterPartDefinition(partViewModel.Name, partBuilder => {
                partBuilder.WithField(fieldViewModel.Name, fieldBuilder => {
                    _contentDefinitionEditorEvents.PartFieldEditorUpdating(fieldBuilder);
                    fieldBuilder.WithDisplayName(fieldViewModel.DisplayName);
                    _contentDefinitionEditorEvents.PartFieldEditorUpdated(fieldBuilder);
                });
            });
        }

        public string GenerateContentTypeNameFromDisplayName(string displayName) {
            displayName = displayName.ToSafeName();

            while (_contentDefinitionManager.GetTypeDefinition(displayName) != null)
                displayName = VersionName(displayName);

            return displayName;
        }

        public string GenerateFieldNameFromDisplayName(string partName, string displayName) {
            IEnumerable<ContentPartFieldDefinition> fieldDefinitions;

            var part = _contentDefinitionManager.GetPartDefinition(partName);
            displayName = displayName.ToSafeName();

            if (part == null) {
                var type = _contentDefinitionManager.GetTypeDefinition(partName);

                if (type == null) {
                    throw new ArgumentException("The part doesn't exist: " + partName);
                }

                var typePart = type.Parts.FirstOrDefault(x => x.PartDefinition.Name == partName);

                // id passed in might be that of a type w/ no implicit field
                if (typePart == null) {
                    return displayName;
                }
                else {
                    fieldDefinitions = typePart.PartDefinition.Fields.ToArray();
                }

            }
            else {
                fieldDefinitions = part.Fields.ToArray();
            }

            while (fieldDefinitions.Any(x => String.Equals(displayName.Trim(), x.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
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
    }
}