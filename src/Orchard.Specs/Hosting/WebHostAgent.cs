using System;

namespace Orchard.Specs.Hosting {
    public class WebHostAgent : MarshalByRefObject
    {
        public SerializableDelegate<Action> Execute(SerializableDelegate<Action> shuttle)
        {
            shuttle.Delegate();
            return shuttle;
        }
    }
}