using System;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public interface IFieldIndexService : IDependency {
        T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName);
        T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName, FieldIndexRecordVersionOptions fieldIndexRecordVersionOption);
        void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType);
        void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType, FieldIndexRecordVersionOptions fieldIndexRecordVersionOption);
    }

    public enum FieldIndexRecordVersionOptions {
        Value,
        LatestValue
    }
}
