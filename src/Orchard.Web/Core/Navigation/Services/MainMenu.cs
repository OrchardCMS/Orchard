using System;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    public class MainMenu : INavigationProvider {
        public string MenuName { get { return "MainMenu"; } }

        public void GetNavigation(NavigationBuilder builder) {
            throw new NotImplementedException();
        }
    }
}
