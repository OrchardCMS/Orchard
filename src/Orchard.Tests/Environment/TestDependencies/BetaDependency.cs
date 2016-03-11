namespace Orchard.Tests.Environment.TestDependencies {

    public interface IBetaDependency : IDependency {
    }

    public class BetaDependency : IBetaDependency {
        public IAlphaDependency Alpha { get; set; }

        public BetaDependency(IAlphaDependency alpha) {
            Alpha = alpha;
        }
    }
}
