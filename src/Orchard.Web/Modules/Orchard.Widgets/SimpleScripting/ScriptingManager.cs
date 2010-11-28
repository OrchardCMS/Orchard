using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Widgets.Services;
using Orchard.Widgets.SimpleScripting.Ast;
using Orchard.Widgets.SimpleScripting.Compiler;

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

        private AbstractSyntaxTree ParseExpression(string expression) {
            return new Parser(expression).Parse();
        }

        private object EvaluateExpression(AstNode root) {
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