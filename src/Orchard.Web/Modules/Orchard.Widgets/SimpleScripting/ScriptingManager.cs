using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.SimpleScripting {
    public interface IScriptingEngine : IDependency {
        bool Matches(string expression);
    }

    public class ScriptingEngine : IScriptingEngine {
        private readonly IEnumerable<IRuleProvider> _ruleProviders;
        private readonly ICacheManager _cacheManager;

        public ScriptingEngine(IEnumerable<IRuleProvider> ruleProviders, ICacheManager cacheManager) {
            _ruleProviders = ruleProviders;
            _cacheManager = cacheManager;
        }

        public bool Matches(string expression) {

            var expressionTree = _cacheManager.Get(expression, ctx => 
                ParseExpression(expression));

            object result = EvaluateExpression(expressionTree.Root);

            return (bool)Convert.ChangeType(result, typeof (bool));
        }

        private ExpressionTree ParseExpression(string expression) {
            return new ExpressionParser(expression).Parse();
        }

        private object EvaluateExpression(ExpressionTree.Expression root) {
            throw new NotImplementedException();
        }

        private object Evaluate(string name, IEnumerable<object> args) {
            var ruleContext = new RuleContext { FunctionName = name, Arguments = args.ToArray() };

            foreach (var ruleProvider in _ruleProviders) {
                ruleProvider.Process(ruleContext);
            }

            return ruleContext.Result;
        }
    }
}