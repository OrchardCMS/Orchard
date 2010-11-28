using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Localization;
using Orchard.Scripting.SimpleScripting.Ast;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Widgets.SimpleScripting {
    public class GlobalMethodContext {
        public string FunctionName { get; set; }
        public IList<object> Arguments { get; set; }
        public object Result { get; set; }
    }

    public interface IGlobalMethodProvider {
        object Process(GlobalMethodContext context);
    }

    public interface IScriptingEngine : IDependency {
        bool Matches(string expression);
    }

    public class ScriptingEngine : IScriptingEngine {
        private readonly IEnumerable<IGlobalMethodProvider> _ruleProviders;
        private readonly ICacheManager _cacheManager;

        public ScriptingEngine(IEnumerable<IGlobalMethodProvider> ruleProviders, ICacheManager cacheManager) {
            _ruleProviders = ruleProviders;
            _cacheManager = cacheManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool Matches(string expression) {
            var expr = _cacheManager.Get(expression, ctx => {
                    var ast = ParseExpression(expression);
                    return new { Tree = ast, Errors = ast.GetErrors().ToList() };
                });

            if (expr.Errors.Any()) {
                //TODO: Collect all errors
                throw new OrchardException(T("Syntax error: {0}", expr.Errors.First().Message));
            }

            var result = EvaluateExpression(expr.Tree);
            if (result.IsError) {
                throw new ApplicationException(result.Error.Message);
            }

            if (!result.IsBool) {
                throw new OrchardException(T("Expression is not a boolean value"));
            }

            return result.BoolValue;
        }

        private AbstractSyntaxTree ParseExpression(string expression) {
            return new Parser(expression).Parse();
        }

        private EvaluationResult EvaluateExpression(AbstractSyntaxTree tree) {
            var context = new EvaluationContext {
                Tree = tree,
                MethodInvocationCallback = (m, args) => Evaluate(m, args)
            };
            return new Interpreter().Evalutate(context);
        }

        private object Evaluate(string name, IEnumerable<object> args) {
            var ruleContext = new GlobalMethodContext() { FunctionName = name, Arguments = args.ToArray() };

            foreach (var ruleProvider in _ruleProviders) {
                ruleProvider.Process(ruleContext);
            }

            return ruleContext.Result;
        }
    }
}