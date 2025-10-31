using System;

namespace Orchard.Scripting.Compiler
{
    public class EvaluationResult
    {
        public EvaluationResult(object value)
        {
            Value = value;
        }

        public static EvaluationResult Result(object value)
        {
            if (value is EvaluationResult)
                throw new InvalidOperationException("Internal error: value cannot be an evaluation result.");
            return new EvaluationResult(value);
        }

        public static EvaluationResult Error(string message)
        {
            return new EvaluationResult(new Error { Message = message });
        }

        public object Value { get; }

        public bool IsError => Value is Error;
        public bool IsNil => IsNull;
        public bool IsNull => Value == null;
        public bool IsBool => Value is bool;
        public bool IsInt32 => Value is int;
        public bool IsString => Value is string;

        public Error ErrorValue => (Error)Value;
        public bool BoolValue => (bool)Value;
        public int Int32Value => (int)Value;
        public string StringValue => (string)Value;

        public override string ToString()
        {
            return IsNull ? "<null>" : Value.ToString();
        }
    }

    public class Error
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return string.Format("Error: {0}", Message);
        }
    }
}