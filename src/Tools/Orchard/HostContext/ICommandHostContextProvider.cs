namespace Orchard.HostContext {
    public interface ICommandHostContextProvider {
        CommandHostContext CreateContext();
        void Shutdown(CommandHostContext context);
    }
}