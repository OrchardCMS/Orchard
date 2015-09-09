namespace Orchard.Alias.Implementation.Updater {
    public interface IAliasUpdateCursor : ISingletonDependency {
        int Cursor { get; set; }
    }
}