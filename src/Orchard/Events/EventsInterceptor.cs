using System.Linq;
using Castle.Core.Interceptor;

namespace Orchard.Events {
    public class EventsInterceptor : IInterceptor {
        private readonly IEventBus _eventBus;

        public EventsInterceptor(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        public void Intercept(IInvocation invocation) {
            var interfaceName = invocation.Method.DeclaringType.Name;
            var methodName = invocation.Method.Name;

            var data = invocation.Method.GetParameters()
                .Select((parameter, index) => new { parameter.Name, Value = invocation.Arguments[index] })
                .ToDictionary(kv => kv.Name, kv => kv.Value.ToString());

            _eventBus.Notify(interfaceName + "_" + methodName, data);
        }
    }
}
