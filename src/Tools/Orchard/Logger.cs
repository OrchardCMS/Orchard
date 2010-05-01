using System;
using System.IO;

namespace Orchard {
    public class Logger : MarshalByRefObject {
        private readonly bool _verbose;
        private readonly TextWriter _output;

        public Logger(bool verbose, TextWriter output) {
            _verbose = verbose;
            _output = output;
        }

        public void LogInfo(string format, params object[] args) {
            if (_verbose) {
                _output.Write("{0}: ", DateTime.Now);
                _output.WriteLine(format, args);
            }
        }
    }
}
