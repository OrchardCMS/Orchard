using System;
using System.Collections.Generic;
using Mono.CSharp;
using Delegate = System.Delegate;

namespace Orchard.Scripting.CSharp.Services {
    public class CSharpService : ICSharpService {

        public Evaluator Engine { get; private set; }
        public IDictionary<string, dynamic> Dictionary { get; private set; }

        public void SetParameter(string name, object value) {
            DemandCompiler();

            Dictionary[name] = value;
            Engine.Run(String.Format("dynamic {0} = dictionary[\"{0}\"]", name));
        }

        public void SetFunction(string name, Delegate value) {
            DemandCompiler();

            Dictionary[name] = value;
            Engine.Run(String.Format("dynamic {0} = dictionary[\"{0}\"]", name));
        }

        public void Run(string script) {
            DemandCompiler();

            Engine.Run(script);
        }

        public object Evaluate(string script) {
            DemandCompiler();

            object result;
            bool resultSet;

            Engine.Evaluate(script, out result, out resultSet);
            if (resultSet) {
                return result;
            }

            return null;
        }

        private void DemandCompiler() {
            if (Engine != null) {
                return;
            }

            Engine = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter()));

            Engine.Run("using System;");
            Engine.Run("using System.Collections.Generic;");
            Engine.Run("using System.Linq;");
            Engine.Run("var dictionary = new Dictionary<string, dynamic>();");
            Dictionary = Engine.Evaluate("dictionary") as IDictionary<string, dynamic>;
        }
    }
}