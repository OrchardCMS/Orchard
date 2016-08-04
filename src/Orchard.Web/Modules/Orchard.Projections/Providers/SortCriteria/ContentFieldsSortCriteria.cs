using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.FieldTypeEditors;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Projections.Providers.SortCriteria {
    public class ContentFieldsSortCriterion : ISortCriterionProvider {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IEnumerable<IFieldTypeEditor> _fieldTypeEditors;

        public ContentFieldsSortCriterion(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IEnumerable<IFieldTypeEditor> fieldTypeEditors) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentFieldDrivers = contentFieldDrivers;
            _fieldTypeEditors = fieldTypeEditors;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeSortCriterionContext describe) {
            foreach(var part in _contentDefinitionManager.ListPartDefinitions()) {
                if(!part.Fields.Any()) {
                    continue;
                }

                var descriptor = describe.For(part.Name + "ContentFields", T("{0} Content Fields", part.Name.CamelFriendly()), T("Content Fields for {0}", part.Name.CamelFriendly()));

                foreach(var field in part.Fields) {
                    var localField = field;
                    var localPart = part;
                    var drivers = _contentFieldDrivers.Where(x => x.GetFieldInfo().Any(fi => fi.FieldTypeName == localField.FieldDefinition.Name)).ToList();

                    var membersContext = new DescribeMembersContext(
                        (storageName, storageType, displayName, description) => {
                            // look for a compatible field type editor
                            IFieldTypeEditor fieldTypeEditor = _fieldTypeEditors.FirstOrDefault(x => x.CanHandle(storageType));

                            descriptor.Element(
                                type: localPart.Name + "." + localField.Name + "." + storageName ?? "",
                                name: new LocalizedString(localField.DisplayName + (displayName != null ? ":" + displayName.Text : "")),
                                description: description ?? T("{0} property for {1}", storageName, localField.DisplayName),
                                sort: context => ApplySortCriterion(context, fieldTypeEditor, storageName, storageType, localPart, localField),
                                display: context => DisplaySortCriterion(context, localPart, localField),
                                form: SortCriterionFormProvider.FormName);
                        });
                    
                    foreach(var driver in drivers) {
                        driver.Describe(membersContext);
                    }
                }
            }
        }

        public void ApplySortCriterion(SortCriterionContext context, IFieldTypeEditor fieldTypeEditor, string storageName, Type storageType, ContentPartDefinition part, ContentPartFieldDefinition field) {
            bool ascending = (bool)context.State.Sort;
            var propertyName = String.Join(".", part.Name, field.Name, storageName ?? "");

            // use an alias with the join so that two filters on the same Field Type wont collide
            var relationship = fieldTypeEditor.GetFilterRelationship(propertyName.ToSafeName());

            // generate the predicate based on the editor which has been used
            Action<IHqlExpressionFactory> predicate = y => y.Eq("PropertyName", propertyName);

            // combines the predicate with a filter on the specific property name of the storage, as implemented in FieldIndexService

            // apply where clause
            context.Query = context.Query.Where(relationship, predicate);
            
            // apply sort
            context.Query = ascending 
                ? context.Query.OrderBy(relationship, x => x.Asc("Value")) 
                : context.Query.OrderBy(relationship, x => x.Desc("Value"));
        }

        public LocalizedString DisplaySortCriterion(SortCriterionContext context, ContentPartDefinition part, ContentPartFieldDefinition fieldDefinition) {
            bool ascending = (bool)context.State.Sort;

            return ascending
                       ? T("Ordered by field {0}, ascending", fieldDefinition.Name)
                       : T("Ordered by field {0}, descending", fieldDefinition.Name);

        }

    }
}