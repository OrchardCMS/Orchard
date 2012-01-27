using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.ViewModels;

namespace Orchard.ContentTypes.Services {
    public interface IContentDefinitionService : IDependency {
        IEnumerable<EditTypeViewModel> GetTypes();
        EditTypeViewModel GetType(string name);
        ContentTypeDefinition AddType(string name, string displayName);
        void AlterType(EditTypeViewModel typeViewModel, IUpdateModel updater);
        void RemoveType(string name, bool deleteContent);
        void AddPartToType(string partName, string typeName);
        void RemovePartFromType(string partName, string typeName);
        string GenerateContentTypeNameFromDisplayName(string displayName);
        string GenerateFieldNameFromDisplayName(string partName, string displayName);

        IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly);
        EditPartViewModel GetPart(string name);
        EditPartViewModel AddPart(CreatePartViewModel partViewModel);
        void AlterPart(EditPartViewModel partViewModel, IUpdateModel updater);
        void RemovePart(string name);

        IEnumerable<ContentFieldInfo> GetFields();
        void AddFieldToPart(string fieldName, string fieldTypeName, string partName);
        void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName);
        void RemoveFieldFromPart(string fieldName, string partName);
    }
}