using System;
using System.Collections.Generic;

namespace Orchard.Specs.Hosting {
    public class MessageSink : MarshalByRefObject {
        readonly IList<string> _messages = new List<string>();

        public void Receive(string message) {
            _messages.Add(message);
        }
    }
}