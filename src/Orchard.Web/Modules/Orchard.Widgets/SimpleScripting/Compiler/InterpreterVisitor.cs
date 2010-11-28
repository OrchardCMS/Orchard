using System;
using Orchard.Widgets.SimpleScripting.Ast;

namespace Orchard.Widgets.SimpleScripting.Compiler {
    public class InterpreterVisitor : AstVisitor {
        public EvaluationResult Evaluate(EvaluationContext context) {
            return Evaluate(context.Tree.Root);
        }

        private EvaluationResult Evaluate(AstNode node) {
            return (EvaluationResult)this.Visit(node);
        }

        public override object VisitConstant(ConstantAstNode node) {
            return Result(node.Value);
        }

        public override object VisitUnary(UnaryAstNode node) {
            var operandValue = Evaluate(node.Operand);
            if (operandValue.IsError)
                return operandValue;

            var operandBoolValue = ConvertToBool(operandValue);
            if (operandBoolValue.IsError)
                return operandBoolValue;

            return Result(!operandBoolValue.BoolValue);
        }

        public override object VisitBinary(BinaryAstNode node) {
            var left = Evaluate(node.Left);
            if (left.IsError)
                return left;

            var right = Evaluate(node.Right);
            if (right.IsError)
                return right;

            switch (node.Token.Kind) {
                case TokenKind.Plus:
                    return EvaluateArithmetic(left, right, (a, b) => Result(a.Int32Value + b.Int32Value));
                case TokenKind.Minus:
                    return EvaluateArithmetic(left, right, (a, b) => Result(a.Int32Value - b.Int32Value));
                case TokenKind.Mul:
                    return EvaluateArithmetic(left, right, (a, b) => Result(a.Int32Value * b.Int32Value));
                case TokenKind.Div:
                    return EvaluateArithmetic(left, right, (a, b) => b.Int32Value == 0 ? Error("Attempted to divide by zero.") : Result(a.Int32Value / b.Int32Value));
                case TokenKind.And:
                    return EvaluateLogical(left, right, (a, b) => Result(a.BoolValue && b.BoolValue));
                case TokenKind.Or:
                    return EvaluateLogical(left, right, (a, b) => Result(a.BoolValue || b.BoolValue));
                default:
                    throw new InvalidOperationException(string.Format("Internal error: binary expression {0} is not supported.", node.Token));
            }
        }

        public override object VisitError(ErrorAstNode node) {
            return Error(node.Message);
        }

        private static EvaluationResult EvaluateArithmetic(EvaluationResult left, EvaluationResult right,
            Func<EvaluationResult, EvaluationResult, EvaluationResult> operation) {
            //TODO: Proper type conversion
            var leftValue = ConvertToInt(left);
            if (leftValue.IsError)
                return leftValue;

            var rightValue = ConvertToInt(right);
            if (rightValue.IsError)
                return rightValue;

            return operation(leftValue, rightValue);
        }

        private static EvaluationResult EvaluateLogical(EvaluationResult left, EvaluationResult right,
            Func<EvaluationResult, EvaluationResult, EvaluationResult> operation) {
            var leftValue = ConvertToBool(left);
            if (leftValue.IsError)
                return leftValue;

            var rightValue = ConvertToBool(right);
            if (rightValue.IsError)
                return rightValue;

            return operation(leftValue, rightValue);
        }

        private static EvaluationResult ConvertToInt(EvaluationResult value) {
            //TODO: Proper type conversion
            if (value.IsInt32)
                return value;

            return Error(string.Format("Value '{0}' is not convertible to an integer.", value));
        }

        private static EvaluationResult ConvertToBool(EvaluationResult value) {
            //TODO: Proper type conversion
            if (value.IsBool)
                return value;

            return Error(string.Format("Value '{0}' is not convertible to a boolean.", value));
        }

        private static EvaluationResult Result(object value) {
            if (value is EvaluationResult)
                throw new InvalidOperationException("Internal error: value cannot be an evaluation result.");
            return new EvaluationResult(value);
        }

        private static EvaluationResult Error(string message) {
            return new EvaluationResult(new Error { Message = message });
        }
    }
}