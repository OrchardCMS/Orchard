using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Security;

namespace Orchard.UI.Navigation {
    public interface INavigationManager : IDependency {
        IEnumerable<MenuItem> BuildMenu(string menuName);
    }

    public class NavigationManager : INavigationManager {
        private readonly IEnumerable<INavigationProvider> _providers;
        private readonly IAuthorizationService _authorizationService;

        public NavigationManager(IEnumerable<INavigationProvider> providers, IAuthorizationService authorizationService) {
            _providers = providers;
            _authorizationService = authorizationService;
        }

        public IUser CurrentUser { get; set; }

        public IEnumerable<MenuItem> BuildMenu(string menuName) {
            return Reduce(Merge(AllSources(menuName))).ToArray();
        }

        private IEnumerable<MenuItem> Reduce(IEnumerable<MenuItem> items) {
            foreach(var item in items) {
                if (!item.Permissions.Any() || 
                    item.Permissions.Any(x=>_authorizationService.CheckAccess(CurrentUser, x))) {
                    yield return new MenuItem {
                        Items = Reduce(item.Items),
                        Permissions = item.Permissions,
                        Position = item.Position,
                        RouteValues = item.RouteValues,
                        Text = item.Text,
                    };
                }
            }
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
                Position = SelectBestPositionValue(items.Select(x => x.Position)),
                Permissions = items.SelectMany(x=>x.Permissions),
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