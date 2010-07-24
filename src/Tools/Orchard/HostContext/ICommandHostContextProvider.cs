namespace OrchardCLI {
    public interface ICommandHostContextProvider {
        CommandHostContext CreateContext();
        void Shutdown(CommandHostContext context);
    }
}