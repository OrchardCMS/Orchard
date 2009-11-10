using Orchard.Models;

namespace Orchard.Security {
    public interface IUser : IModel {
        string Name { get; }
    }
}
