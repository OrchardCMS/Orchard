using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Castle.Core.Internal;
using Moq;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class MenuFilterTests {
        private const string FirstLevel1Action = "FirstLevel1";
        private const string SecondLevel1Action = "SecondLevel1";
        private const string SecondLevel2Action = "SecondLevel2";
        private const string ThirdLevel1Action = "ThirdLevel1";
        private const string ThirdLevel2Action = "ThirdLevel2";
        private const string ThirdLevel3Action = "ThirdLevel3";
        private const string ThirdLevel4Action = "ThirdLevel4";
        private const string FourthLevel1Action = "FourthLevel1";
        private const string FourthLevel2Action = "FourthLevel2";
        private const string FourthLevel3Action = "FourthLevel3";
        private const string FourthLevel4Action = "FourthLevel4";

        [Test]
        public void MockNavManagerWorks() {
            var main = GetNavigationManager().Object.BuildMenu("main");
            Assert.That(main.Count(), Is.EqualTo(1));
        }

        [Test]
        public void FindSelectedPathScenario2() {
            NavigationBuilder navigationBuilder = BuildMenuScenario2();
            IEnumerable<MenuItem> menuItems = navigationBuilder.Build();

            MenuItem firstLevel1 = FindMenuItem(menuItems, "X");
            MenuItem secondLevel2 = FindMenuItem(menuItems, "B");
            MenuItem thirdLevel2 = FindMenuItem(menuItems, "D");
            MenuItem fourthLevel3 = FindMenuItem(menuItems, "G");
            RouteData fourthLevel3RouteData = GetRouteData(fourthLevel3);

            Stack<MenuItem> selectionStack = MenuFilterAccessor.FindSelectedPathAccessor(menuItems, fourthLevel3RouteData);
            Assert.That(selectionStack.Pop(), Is.EqualTo(firstLevel1));
            Assert.That(selectionStack.Pop(), Is.EqualTo(secondLevel2));
            Assert.That(selectionStack.Pop(), Is.EqualTo(thirdLevel2));
            Assert.That(selectionStack.Pop(), Is.EqualTo(fourthLevel3));
            Assert.That(selectionStack.Count, Is.EqualTo(0));
        }

        [Test]
        public void FindParentLocalTaskScenario2() {
            NavigationBuilder navigationBuilder = BuildMenuScenario2();
            IEnumerable<MenuItem> menuItems = navigationBuilder.Build();

            MenuItem fourthLevel3 = FindMenuItem(menuItems, "G");
            RouteData fourthLevel3RouteData = GetRouteData(fourthLevel3);
            Stack<MenuItem> selectedPath = MenuFilterAccessor.FindSelectedPathAccessor(menuItems, fourthLevel3RouteData);

            MenuItem parentNode = MenuFilterAccessor.FindParentLocalTaskAccessor(selectedPath);

            Assert.That(parentNode, Is.EqualTo(FindMenuItem(menuItems, "B")));
        } 

        private static Mock<INavigationManager> GetNavigationManager() {
            var mainMenu = new[] { new MenuItem { Text = new LocalizedString("The Main Menu") } };
            var adminMenu = new[] { new MenuItem { Text = new LocalizedString("The Admin Menu") } };
            var navigationManager = new Mock<INavigationManager>();
            navigationManager.Setup(x => x.BuildMenu("main")).Returns(mainMenu);
            navigationManager.Setup(x => x.BuildMenu("admin")).Returns(adminMenu);
            return navigationManager;
        }

        private static NavigationBuilder BuildMenuScenario1() {
            NavigationBuilder navigationBuilder = new NavigationBuilder();
            navigationBuilder.Add(new LocalizedString("X"), "0",
                menu => menu
                    .Add(new LocalizedString("A"), "0", subMenu => subMenu.Action("Index", "Admin", new { area = "Area" })
                        .Add(new LocalizedString("B"), "0", item => item.Action("Index", "Admin", new { area = "Area" }))
                        .Add(new LocalizedString("C"), "1", item => item.Action("Index", "Admin", new { area = "Area" }).LocalNav())))
                    .Add(new LocalizedString("D"), "1", subMenu => subMenu.Action("Index", "Admin", new { area = "Area" }).LocalNav()
                        .Add(new LocalizedString("E"), "0", item => item.Action("Index", "Admin", new { area = "Area" }))
                        .Add(new LocalizedString("F"), "1", item => item.Action("Index", "Admin", new { area = "Area" }).LocalNav()));

            return navigationBuilder;
        }

        private static NavigationBuilder BuildMenuScenario2() {
            NavigationBuilder navigationBuilder = new NavigationBuilder();
            navigationBuilder.Add(new LocalizedString("X"), "0",
                menu => menu
                    .Add(new LocalizedString("A"), "0", item => item.Action(SecondLevel1Action, "Admin", new { area = "Area" }))
                    .Add(new LocalizedString("B"), "1",
                        subMenu => subMenu
                            .Add(new LocalizedString("C"), "0", item => item.Action(ThirdLevel1Action, "Admin", new { area = "Area" }).LocalNav())
                            .Add(new LocalizedString("D"), "1",
                                subSubMenu => subSubMenu.LocalNav()
                                    .Add(new LocalizedString("E"), "0", item => item.Action(FourthLevel1Action, "Admin", new { area = "Area" }).LocalNav())
                                    .Add(new LocalizedString("F"), "1", item => item.Action(FourthLevel2Action, "Admin", new { area = "Area" }).LocalNav())
                                    .Add(new LocalizedString("G"), "2", item => item.Action(FourthLevel3Action, "Admin", new { area = "Area" }))
                                    .Add(new LocalizedString("W"), "3", item => item.Action(FourthLevel4Action, "Admin", new { area = "Area" })))));

            return navigationBuilder;
        }

        protected static MenuItem FindMenuItem(IEnumerable<MenuItem> menuItems, string text) {
            Queue<MenuItem> remainingItems = new Queue<MenuItem>(menuItems);

            while (remainingItems.Count > 0) {
                MenuItem currentMenuItem = remainingItems.Dequeue();

                if (currentMenuItem.Text.TextHint.Equals(text)) {
                    return currentMenuItem;
                }

                currentMenuItem.Items.ForEach(remainingItems.Enqueue);
            }

            return null;
        }

        private static RouteData GetRouteData(MenuItem menuItem) {
            RouteData routeData = new RouteData();
            routeData.Values["area"] = menuItem.RouteValues["area"];
            routeData.Values["controller"] = menuItem.RouteValues["controller"];
            routeData.Values["action"] = menuItem.RouteValues["action"];

            return routeData;
        }

        private class MenuFilterAccessor : MenuFilter {
            public MenuFilterAccessor(INavigationManager navigationManager,
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory) :
                base(navigationManager, workContextAccessor, shapeFactory) {}

            public static Stack<MenuItem> FindSelectedPathAccessor(IEnumerable<MenuItem> menuItems, RouteData currentRouteData) {
                return NavigationHelper.SetSelectedPath(menuItems, null, currentRouteData);
            }

            public static MenuItem FindParentLocalTaskAccessor(Stack<MenuItem> selectedPath) {
                return NavigationHelper.FindParentLocalTask(selectedPath);
            }
        }
    }
}
