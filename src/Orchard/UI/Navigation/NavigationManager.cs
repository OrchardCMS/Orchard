using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Orchard.UI.Navigation {
    public interface INavigationManager : IDependency {
        IEnumerable<MenuItem> BuildMenu(string menuName);
    }

    public class NavigationManager : INavigationManager {
        private readonly IEnumerable<INavigationProvider> _providers;

        public NavigationManager(IEnumerable<INavigationProvider> providers) {
            _providers = providers;
        }

        public IEnumerable<MenuItem> BuildMenu(string menuName) {
            return Merge(AllSources(menuName)).ToArray();
        }

        private IEnumerable<IEnumerable<MenuItem>> AllSources(string menuName) {
            foreach (var provider in _providers) {
                if (provider.MenuName == menuName) {
                    var builder = new NavigationBuilder();
                    provider.GetNavigation(builder);
                    yield return builder.Build();
                }
            }
        }

        private static IEnumerable<MenuItem> Merge(IEnumerable<IEnumerable<MenuItem>> sources) {
            var comparer = new MenuItemComparer();
            var orderer = new PositionComparer();

            return sources.SelectMany(x => x).ToArray()
                .GroupBy(key => key, (key, items) => Join(items), comparer)
                .OrderBy(item => item.Position, orderer);
        }

        static MenuItem Join(IEnumerable<MenuItem> items) {
            if (items.Count() < 2)
                return items.Single();

            var joined = new MenuItem {
                Text = items.First().Text,
                RouteValues = items.First().RouteValues,
                Items = Merge(items.Select(x => x.Items)).ToArray(),
                Position = SelectBestPositionValue(items.Select(x => x.Position))
            };
            return joined;
        }

        private static string SelectBestPositionValue(IEnumerable<string> positions) {
            var comparer = new PositionComparer();
            return positions.Aggregate(string.Empty,
                                       (agg, pos) =>
                                       string.IsNullOrEmpty(agg)
                                           ? pos
                                           : string.IsNullOrEmpty(pos)
                                                 ? agg
                                                 : comparer.Compare(agg, pos) < 0 ? agg : pos);
        }
    }
}