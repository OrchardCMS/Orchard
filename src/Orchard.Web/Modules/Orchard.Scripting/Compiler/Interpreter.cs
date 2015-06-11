using System;
using System.Collections.Generic;
using Orchard.Scripting.Ast;

namespace Orchard.Scripting.Compiler {
    public class Interpreter {
        public EvaluationResult Evalutate(EvaluationContext context) {
            return new InterpreterVisitor(context).Evaluate();
        }
    }

    public class EvaluationContext {
        public AbstractSyntaxTree Tree { get; set; }
        public Func<string, IList<object>, object> MethodInvocationCallback { get; set; }
    }
}