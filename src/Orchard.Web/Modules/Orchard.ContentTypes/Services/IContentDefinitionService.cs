using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.ViewModels;

namespace Orchard.ContentTypes.Services {
    public interface IContentDefinitionService : IDependency {
        IEnumerable<EditTypeViewModel> GetTypes();
        EditTypeViewModel GetType(string name);
        EditTypeViewModel AddType(CreateTypeViewModel typeViewModel);
        void AlterType(EditTypeViewModel typeViewModel, IUpdateModel updater);
        void RemoveType(string name);
        void AddPartToType(string partName, string typeName);
        void RemovePartFromType(string partName, string typeName);

        IEnumerable<EditPartViewModel> GetParts();
        EditPartViewModel GetPart(string name);
        EditPartViewModel AddPart(CreatePartViewModel partViewModel);
        void AlterPart(EditPartViewModel partViewModel, IUpdateModel updater);
        void RemovePart(string name);

        IEnumerable<ContentFieldInfo> GetFields();
        void AddFieldToPart(string fieldName, string fieldTypeName, string partName);
        void RemoveFieldFromPart(string fieldName, string partName);
    }
}