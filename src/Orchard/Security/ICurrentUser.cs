namespace Orchard.Security {
    public interface ICurrentUser {
        IUser CurrentUser { get; set; }
    }
}