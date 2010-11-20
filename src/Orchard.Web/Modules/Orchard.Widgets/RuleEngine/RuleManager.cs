using System.Collections.Generic;
using System.Linq;
using Orchard.Scripting;
using Orchard.Scripting.Services;
using Orchard.UI.Widgets;

namespace Orchard.Widgets.RuleEngine {
    public class RuleManager : IRuleManager {
        private readonly IEnumerable<IRuleProvider> _ruleProviders;
        private readonly IScriptingManager _scriptingManager;

        public RuleManager(IEnumerable<IRuleProvider> ruleProviders, IScriptingManager scriptingManager) {
            _ruleProviders = ruleProviders;
            _scriptingManager = scriptingManager;
        }

        public bool Matches(string expression) {
            object execContextType = _scriptingManager.ExecuteExpression(@"
                                        class ExecContext
	                                        def execute(callbacks, text)
		                                        @callbacks = callbacks;
		                                        temp = instance_eval(text.to_s);
		                                        @callbacks = 0;
                                                return temp;
	                                        end

	                                        def method_missing(name, *args, &block)
		                                        @callbacks.send(name, args, &block);
                                            end
                                        end
                                        ExecContext
                                        ");

            object execContext = _scriptingManager.ExecuteOperation(ops => ops.CreateInstance(execContextType));
            return _scriptingManager.ExecuteOperation(ops => ops.InvokeMember(execContext, "execute", new CallbackApi(this), expression));
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