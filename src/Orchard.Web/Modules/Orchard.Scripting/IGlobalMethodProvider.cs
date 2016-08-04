using System.Collections.Generic;

namespace Orchard.Scripting {
    public interface IGlobalMethodProvider {
        void Process(GlobalMethodContext context);
    }

    public class GlobalMethodContext {
        public string FunctionName { get; set; }
        public IList<object> Arguments { get; set; }
        public object Result { get; set; }
    }
}