namespace Orchard.HostContext {
    public interface ICommandHostContextProvider {
        CommandHostContext CreateContext(bool interactive);
        void Shutdown(CommandHostContext context);
    }
}