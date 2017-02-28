namespace Orchard.Glimpse.Interceptors {
    public interface IGlimpseMessageInterceptor : IDependency {
        void MessageReceived<TMessage>(TMessage message) where TMessage : class;
    }
}