namespace Orchard.Glimpse.Interceptors {
    public abstract class GlimpseMessageInterceptor<T> : IGlimpseMessageInterceptor where T : class {
        public void MessageReceived<TMessage>(TMessage message) where TMessage : class {
            var typedMessage = message as T;

            if (typedMessage != null) {
                ProcessMessage(typedMessage);
            }
        }

        public abstract void ProcessMessage(T message);
    }
}