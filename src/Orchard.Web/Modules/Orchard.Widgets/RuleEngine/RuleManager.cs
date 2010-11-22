using System.Collections.Generic;
using System.Linq;
using Orchard.Scripting.Services;
using Orchard.Widgets.Services;
using Microsoft.Scripting.Hosting;
using Orchard.Caching;

namespace Orchard.Widgets.RuleEngine {
    public class RuleManager : IRuleManager {
        private readonly IEnumerable<IRuleProvider> _ruleProviders;
        private readonly IScriptingManager _scriptingManager;
        private readonly ICacheManager _cacheManager;

        public RuleManager(IEnumerable<IRuleProvider> ruleProviders, IScriptingManager scriptingManager, ICacheManager cacheManager) {
            _ruleProviders = ruleProviders;
            _scriptingManager = scriptingManager;
            _cacheManager = cacheManager;
        }

        public bool Matches(string expression) {
            object execContextType = _cacheManager.Get("---", ctx => (object)_scriptingManager.ExecuteExpression(@"
class ExecBlock
    def initialize(callbacks)
        @callbacks = callbacks
    end
    def method_missing(name, *args, &block)
        @callbacks.send(name, args, &block);
    end
end
class ExecContext
    class << self
        def alloc(thing)
            instance_eval 'self.new {' + thing + '}'
        end
    end
    def initialize(&block)
        @block = block
    end
    def evaluate(callbacks)
      ExecBlock.new(callbacks).instance_eval(&@block)
    end
end
ExecContext
                                        "));
            var ops = _cacheManager.Get("----", ctx => (ObjectOperations)_scriptingManager.ExecuteOperation(x => x));
            object execContext = _cacheManager.Get(expression, ctx => (object)ops.InvokeMember(execContextType, "alloc", expression));
            dynamic result = ops.InvokeMember(execContext, "evaluate", new CallbackApi(this));
            return result;
        }

        public class CallbackApi {
            private readonly RuleManager _ruleManager;

            public CallbackApi(RuleManager ruleManager) {
                _ruleManager = ruleManager;
            }

            public object send(string name, IList<object> args) {
                return _ruleManager.Evaluate(name, args);
            }
        }

        private object Evaluate(string name, IList<object> args) {
            RuleContext ruleContext = new RuleContext { FunctionName = name, Arguments = args.ToArray() };

            foreach (var ruleProvider in _ruleProviders) {
                ruleProvider.Process(ruleContext);
            }

            return ruleContext.Result;
        }
    }
}