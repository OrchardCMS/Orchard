using Orchard.ContentManagement;

namespace Orchard.Security {
    /// <summary>
    /// Interface provided by the "user" model. 
    /// </summary>
    public interface IUser : IContent {
        int Id { get; }
        string UserName { get; }
        string Email { get; }
    }
}
