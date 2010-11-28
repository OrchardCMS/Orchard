using System;
using System.Collections.Generic;
using Orchard.Widgets.SimpleScripting.Ast;

namespace Orchard.Widgets.SimpleScripting.Compiler {
    public class Interpreter {
        public EvaluationResult Evalutate(EvaluationContext context) {
            return new InterpreterVisitor(context).Evaluate();
        }
    }

    public class EvaluationContext {
        public AbstractSyntaxTree Tree { get; set; }
        public Func<object, string, IList<object>> MethodInvocationCallback { get; set; }

    }

    public class EvaluationResult<T> {
        public bool HasErrors { get; set; }
        public T Value { get; set; }
    }

    public class EvaluationResult : EvaluationResult<object> {
    }
}