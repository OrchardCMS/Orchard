using System;
using System.Threading;

namespace Orchard.Core.Containers.Services {
    public interface IRandomizer : ISingletonDependency {
        int Next(int minValue, int maxValue);
        int Next(int maxValue);
    }

    public class Randomizer : IRandomizer {
        private readonly Lazy<Random> _randomField = new Lazy<Random>(() => new Random(Guid.NewGuid().GetHashCode()), LazyThreadSafetyMode.ExecutionAndPublication);
        private Random Random { get { return _randomField.Value; } }

        public int Next(int minValue, int maxValue) {
            return Random.Next(minValue, maxValue);
        }

        public int Next(int maxValue) {
            return Random.Next(maxValue);
        }
    }
}