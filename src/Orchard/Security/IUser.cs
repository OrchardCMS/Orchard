using Orchard.ContentManagement;

namespace Orchard.Security {
    /// <summary>
    /// Interface provided by the "User" model. 
    /// </summary>
    public interface IUser : IContent {
        string UserName { get; }
        string Email { get; }
    }
}
