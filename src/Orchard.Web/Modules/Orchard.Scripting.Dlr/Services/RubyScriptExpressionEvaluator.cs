using System.Collections.Generic;
using System.Linq;
using Microsoft.Scripting.Hosting;
using Orchard.Caching;

namespace Orchard.Scripting.Dlr.Services {
    public class RubyScriptExpressionEvaluator : IScriptExpressionEvaluator {
        private readonly IScriptingManager _scriptingManager;
        private readonly ICacheManager _cacheManager;

        public RubyScriptExpressionEvaluator(IScriptingManager scriptingManager, ICacheManager cacheManager) {
            _scriptingManager = scriptingManager;
            _cacheManager = cacheManager;
        }

        public object Evaluate(string expression, IEnumerable<IGlobalMethodProvider> providers) {
            object execContextType = _cacheManager.Get("---", true, ctx => (object)_scriptingManager.ExecuteExpression(@"
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
            var ops = _cacheManager.Get("----", true, ctx => (ObjectOperations)_scriptingManager.ExecuteOperation(x => x));
            object execContext = _cacheManager.Get(expression, ctx => (object)ops.InvokeMember(execContextType, "alloc", expression));
            dynamic result = ops.InvokeMember(execContext, "evaluate", new CallbackApi(this, providers));
            return ConvertRubyValue(result);
        }

        private object ConvertRubyValue(object result) {
            if (result is IronRuby.Builtins.MutableString)
                return result.ToString();
            return result;
        }

        public class CallbackApi {
            private readonly RubyScriptExpressionEvaluator _ruleManager;
            private readonly IEnumerable<IGlobalMethodProvider> _providers;

            public CallbackApi(RubyScriptExpressionEvaluator ruleManager, IEnumerable<IGlobalMethodProvider> providers) {
                _ruleManager = ruleManager;
                _providers = providers;
            }

            public object send(string name, IList<object> args) {
                return _ruleManager.Evaluate(_providers, name, args);
            }
        }

        private object Evaluate(IEnumerable<IGlobalMethodProvider> providers, string name, IEnumerable<object> args) {
            var ruleContext = new GlobalMethodContext {
                FunctionName = name, 
                Arguments = args.Select(v => ConvertRubyValue(v)).ToArray()
            };

            foreach (var ruleProvider in providers) {
                ruleProvider.Process(ruleContext);
            }

            return ruleContext.Result;
        }
    }
}