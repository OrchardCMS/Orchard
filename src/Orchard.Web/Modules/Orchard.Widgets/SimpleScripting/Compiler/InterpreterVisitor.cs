using System;
using Orchard.Widgets.SimpleScripting.Ast;

namespace Orchard.Widgets.SimpleScripting.Compiler {
    public class InterpreterVisitor : AstVisitor {
        private readonly EvaluationContext _context;

        public InterpreterVisitor(EvaluationContext context) {
            _context = context;
        }

        public EvaluationResult Evaluate() {
            return Evaluate(_context.Tree.Root);
        }

        public EvaluationResult Evaluate(AstNode node) {
            return (EvaluationResult)this.Visit(node);
        }

        public override object VisitConstant(ConstantAstNode node) {
            return new EvaluationResult { Value = node.Value };
        }

        public override object VisitBinary(BinaryAstNode node) {
            var left = Evaluate(node.Left);
            if (left.HasErrors)
                return left;

            var right = Evaluate(node.Right);
            if (right.HasErrors)
                return right;

            switch (node.Token.Kind) {
                case TokenKind.Plus:
                    return EvaluateArithmetic(left, right, (a, b) => a + b);
                case TokenKind.Minus:
                    return EvaluateArithmetic(left, right, (a, b) => a - b);
                case TokenKind.Mul:
                    return EvaluateArithmetic(left, right, (a, b) => a * b);
                case TokenKind.Div:
                    //TODO: divide by zero?
                    return EvaluateArithmetic(left, right, (a, b) => a / b);
                case TokenKind.And:
                    return EvaluateLogical(left, right, (a, b) => a && b);
                case TokenKind.Or:
                    return EvaluateLogical(left, right, (a, b) => a || b);

            }

            return new EvaluationResult {HasErrors = true};
        }

        private EvaluationResult EvaluateArithmetic(EvaluationResult left, EvaluationResult right, Func<int, int, int> operation) {
            //TODO: Proper type conversion
            var leftValue = ConvertToInt(left);
            var rightValue = ConvertToInt(right);

            return new EvaluationResult { Value = operation(leftValue.Value, rightValue.Value) };
        }

        private EvaluationResult EvaluateLogical(EvaluationResult left, EvaluationResult right, Func<bool, bool, bool> operation) {
            //TODO: Proper type conversion
            var leftValue = ConvertToBool(left);
            var rightValue = ConvertToBool(right);

            return new EvaluationResult { Value = operation(leftValue.Value, rightValue.Value) };
        }


        private EvaluationResult<int> ConvertToInt(EvaluationResult value) {
            if (value.Value is int)
                return new EvaluationResult<int> { Value = (int)value.Value };

            return new EvaluationResult<int>() { HasErrors = true, Value = 0 };
        }

        private EvaluationResult<bool> ConvertToBool(EvaluationResult value) {
            if (value.Value is bool)
                return new EvaluationResult<bool>() { Value = (bool)value.Value };

            if (value.Value is int)
                return new EvaluationResult<bool>() { Value = ((int)value.Value) != 0 };

            return new EvaluationResult<bool>() { HasErrors = true, Value = false };
        }
    }
}