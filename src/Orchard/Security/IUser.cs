using Orchard.Models;

namespace Orchard.Security {
    /// <summary>
    /// Interface provided by the "user" model. 
    /// </summary>
    public interface IUser : IModel {
        string UserName { get; }
        string Email { get; }
    }
}
