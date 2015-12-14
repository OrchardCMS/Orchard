using System;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public interface IFieldIndexService : IDependency {
        void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType);
        T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName);
    }
}
