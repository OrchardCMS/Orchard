using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.FilterEditors.Forms;
using Orchard.Projections.Models;

namespace Orchard.Projections.FieldTypeEditors
{
    /// <summary>
    /// <see cref="IFieldTypeEditor"/> implementation for integer properties
    /// </summary>
    public class IntegerFieldTypeEditor : IFieldTypeEditor
    {
        public Localizer T { get; set; }

        public IntegerFieldTypeEditor()
        {
            T = NullLocalizer.Instance;
        }

        public bool CanHandle(Type storageType)
        {
            return new[] {
                typeof(byte),
                typeof(sbyte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
            }.Contains(storageType);
        }

        public string FormName => NumericFilterForm.FormName;

        public Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState)
        {
            return NumericFilterForm.GetFilterPredicate(
                formState,
                this.GetQueryVersionScope((string)formState.VersionScope).ToVersionedFieldIndexColumnName());
        }

        public LocalizedString DisplayFilter(string fieldName, string storageName, dynamic formState)
        {
            return NumericFilterForm.DisplayFilter(fieldName + " " + storageName, formState, T);
        }

        public Action<IAliasFactory> GetFilterRelationship(string aliasName)
        {
            return x => x.ContentPartRecord<FieldIndexPartRecord>().Property("IntegerFieldIndexRecords", aliasName);
        }
    }
}