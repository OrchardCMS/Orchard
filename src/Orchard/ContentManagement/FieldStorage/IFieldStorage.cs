using System;

namespace Orchard.ContentManagement.FieldStorage {
    public interface IFieldStorage {
        Func<string, string> Getter { get; }
        Action<string, string> Setter { get; }
    }
}