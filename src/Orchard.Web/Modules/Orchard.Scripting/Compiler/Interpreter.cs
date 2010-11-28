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

    public class EvaluationResult {
        private readonly object _value;

        public EvaluationResult(object value) {
            _value = value;
        }

        public object Value { get { return _value; } }

        public bool IsError { get { return Value is Error; } }
        public bool IsNil { get { return Value is Nil; } }
        public bool IsNull { get { return Value == null; } }
        public bool IsBool { get { return Value is bool; } }
        public bool IsInt32 { get { return Value is int; } }
        public bool IsString { get { return Value is string; } }

        public Error Error { get { return (Error)Value; } }
        public bool BoolValue { get { return (bool)Value; } }
        public int Int32Value { get { return (int)Value; } }
        public string StringValue { get { return (string)Value; } }

        public override string ToString() {
            if (IsNull)
                return "<null>";

            return Value.ToString();
        }
    }

    public class Error {
        public string Message { get; set; }

        public override string ToString() {
            return string.Format("Error: {0}", Message);
        }
    }

    public class Nil {
        public override string ToString() {
            return "nil";
        }
    }
}