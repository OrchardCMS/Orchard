namespace Orchard.UI.Widgets {
    public interface IRuleProvider : IDependency {
        void Process(RuleContext ruleContext);
    }
}

