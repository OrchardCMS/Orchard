using System.Collections.Generic;
using System.Linq;
using Orchard.Scripting;
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
            _scriptingManager.SetVariable("callbacks", new CallbackApi(this));
            dynamic execContext = _scriptingManager.ExecuteExpression(@"
                                        class ExecContext
	                                        def initialize(callbacks)
		                                        @callbacks = callbacks;
	                                        end
	
	                                        def execute(text)
		                                        instance_eval(text.to_s);
	                                        end

	                                        def method_missing(name, *args, &block)
		                                        @callbacks.send(name, args, &block);
                                            end
                                        end
                                        ExecContext.new(callbacks)");
            return execContext.execute(expression);
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
            RuleContext ruleContext = new RuleContext {FunctionName = name, Arguments = args.ToArray()};

            foreach (var ruleProvider in _ruleProviders) {
                ruleProvider.Process(ruleContext);
            }

            return ruleContext.Result;
        }
    }
}