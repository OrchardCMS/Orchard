using System;

namespace Orchard.ContentManagement.Drivers.FieldStorage {
    public interface IFieldStorage {
        Func<string, string> Getter { get; }
        Action<string, string> Setter { get; }
    }
}