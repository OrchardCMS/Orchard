using System;
using Orchard.Events;

namespace Orchard.ContentManagement.FieldStorage
{
    public class FieldStorageEventContext
    {
        public IContent Content { get; set; }
        public string PartName { get; set; }
        public string FieldName { get; set; }
        public string ValueName { get; set; }
        public object Value { get; set; }
        public Type ValueType { get; set; }
    }

    public interface IFieldStorageEvents : IEventHandler
    {
        void SetCalled(FieldStorageEventContext context);
    }
}
