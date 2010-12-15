namespace Orchard.Widgets.Services {
    public interface IRuleProvider : IDependency {
        void Process(RuleContext ruleContext);
    }
}