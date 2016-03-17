using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Orchard.UI.Navigation {
    public static class NavigationHelper {

        /// <summary>
        /// Populates the menu shapes.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The menu parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItems">The current level to populate.</param>
        public static void PopulateMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems) {
            foreach (MenuItem menuItem in menuItems) {
                dynamic menuItemShape = BuildMenuItemShape(shapeFactory, parentShape, menu, menuItem);

                if (menuItem.Items != null && menuItem.Items.Any()) {
                    PopulateMenu(shapeFactory, menuItemShape, menu, menuItem.Items);
                }

                parentShape.Add(menuItemShape, menuItem.Position);
            }
        }

        /// <summary>
        /// Populates the local menu starting from the first non local task parent.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The menu parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="selectedPath">The selection path.</param>
        public static void PopulateLocalMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, Stack<MenuItem> selectedPath) {
            MenuItem parentMenuItem = FindParentLocalTask(selectedPath);

            // find childs tabs and expand them
            if (parentMenuItem != null && parentMenuItem.Items != null && parentMenuItem.Items.Any()) {
                PopulateLocalMenu(shapeFactory, parentShape, menu, parentMenuItem.Items);
            }
        }

        /// <summary>
        /// Populates the local menu shapes.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The menu parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItems">The current level to populate.</param>
        public static void PopulateLocalMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems) {
            foreach (MenuItem menuItem in menuItems) {
                dynamic menuItemShape = BuildLocalMenuItemShape(shapeFactory, parentShape, menu, menuItem);

                if (menuItem.Items != null && menuItem.Items.Any()) {
                    PopulateLocalMenu(shapeFactory, menuItemShape, menu, menuItem.Items);
                }

                parentShape.Add(menuItemShape, menuItem.Position);
            }
        }

        /// <summary>
        /// Identifies the currently selected path, starting from the selected node.
        /// </summary>
        /// <param name="menuItems">All the menuitems in the navigation menu.</param>
        /// <param name="currentRequest">The currently executed request if any</param>
        /// <param name="currentRouteData">The current route data.</param>
        /// <returns>A stack with the selection path being the last node the currently selected one.</returns>
        public static Stack<MenuItem> SetSelectedPath(IEnumerable<MenuItem> menuItems, HttpRequestBase currentRequest, RouteData currentRouteData) {
            return SetSelectedPath(menuItems, currentRequest, currentRouteData.Values);
        }

        /// <summary>
        /// Identifies the currently selected path, starting from the selected node.
        /// </summary>
        /// <param name="menuItems">All the menuitems in the navigation menu.</param>
        /// <param name="currentRequest">The currently executed request if any</param>
        /// <param name="currentRouteData">The current route data.</param>
        /// <returns>A stack with the selection path being the last node the currently selected one.</returns>
        public static Stack<MenuItem> SetSelectedPath(IEnumerable<MenuItem> menuItems, HttpRequestBase currentRequest, RouteValueDictionary currentRouteData) {
            // doing route data comparison first and if that fails, fallback to string-based URL lookup
            var path = SetSelectedPath(menuItems, currentRequest, currentRouteData, false) 
                    ?? SetSelectedPath(menuItems, currentRequest, currentRouteData, true);

            return path;
        }

        /// <summary>
        /// Identifies the currently selected path, starting from the selected node.
        /// </summary>
        /// <param name="menuItems">All the menuitems in the navigation menu.</param>
        /// <param name="currentRequest">The currently executed request if any</param>
        /// <param name="currentRouteData">The current route data.</param>
        /// <param name="compareUrls">Should compare raw string URLs instead of route data.</param>
        /// <returns>A stack with the selection path being the last node the currently selected one.</returns>
        private static Stack<MenuItem> SetSelectedPath(IEnumerable<MenuItem> menuItems, HttpRequestBase currentRequest, RouteValueDictionary currentRouteData, bool compareUrls) {
            foreach (MenuItem menuItem in menuItems) {
                Stack<MenuItem> selectedPath = SetSelectedPath(menuItem.Items, currentRequest, currentRouteData, compareUrls);
                if (selectedPath != null) {
                    menuItem.Selected = true;
                    selectedPath.Push(menuItem);
                    return selectedPath;
                }

                // compare route values (if any) first
                // if URL string comparison is used it means all previous route matches failed, thus no need to do them twice
                bool match = !compareUrls && menuItem.RouteValues != null && RouteMatches(menuItem.RouteValues, currentRouteData);

                // if route match failed, try comparing URL strings, if
                if (currentRequest != null && !match && compareUrls) {
                    string appPath = currentRequest.ApplicationPath ?? "/";
                    string requestUrl = currentRequest.Path.StartsWith(appPath) ? currentRequest.Path.Substring(appPath.Length) : currentRequest.Path;

                    string modelUrl = menuItem.Href.Replace("~/", appPath);
                    modelUrl = modelUrl.StartsWith(appPath) ? modelUrl.Substring(appPath.Length) : modelUrl;

                    if (requestUrl.Equals(modelUrl, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(modelUrl) && requestUrl.StartsWith(modelUrl + "/", StringComparison.OrdinalIgnoreCase))) {
                        match = true;
                    }
                }

                if (match) {
                    menuItem.Selected = true;

                    selectedPath = new Stack<MenuItem>();
                    selectedPath.Push(menuItem);
                    return selectedPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first level in the selection path, starting from the bottom, that is not a local task.
        /// </summary>
        /// <param name="selectedPath">The selection path stack. The bottom node is the currently selected one.</param>
        /// <returns>The first node, starting from the bottom, that is not a local task. Otherwise, null.</returns>
        public static MenuItem FindParentLocalTask(Stack<MenuItem> selectedPath) {
            if (selectedPath != null) {
                MenuItem parentMenuItem = selectedPath.Pop();
                if (parentMenuItem != null) {
                    while (selectedPath.Count > 0) {
                        MenuItem currentMenuItem = selectedPath.Pop();
                        if (currentMenuItem.LocalNav) {
                            return parentMenuItem;
                        }

                        parentMenuItem = currentMenuItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines if a menu item corresponds to a given route.
        /// </summary>
        /// <param name="itemValues">The menu item.</param>
        /// <param name="requestValues">The route data.</param>
        /// <returns>True if the menu item's action corresponds to the route data; false otherwise.</returns>
        public static bool RouteMatches(RouteValueDictionary itemValues, RouteValueDictionary requestValues) {
            if (itemValues == null && requestValues == null) {
                return true;
            }
            if (itemValues == null || requestValues == null) {
                return false;
            }
            if (itemValues.Keys.Any(key => requestValues.ContainsKey(key) == false)) {
                return false;
            }
            return itemValues.Keys.All(key => string.Equals(Convert.ToString(itemValues[key]), Convert.ToString(requestValues[key]), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Builds a menu item shape.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItem">The menu item to build the shape for.</param>
        /// <returns>The menu item shape.</returns>
        public static dynamic BuildMenuItemShape(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem) {
            var menuItemShape = shapeFactory.MenuItem()
                .Text(menuItem.Text)
                .IdHint(menuItem.IdHint)
                .Href(menuItem.Href)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .LocalNav(menuItem.LocalNav)
                .Selected(menuItem.Selected)
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape)
                .Content(menuItem.Content)
                .Level(menuItem.Level);

            foreach (var className in menuItem.Classes)
                menuItemShape.Classes.Add(className);

            return menuItemShape;
        }

        /// <summary>
        /// Builds a local menu item shape.
        /// </summary>
        /// <param name="shapeFactory">The shape factory.</param>
        /// <param name="parentShape">The parent shape.</param>
        /// <param name="menu">The menu shape.</param>
        /// <param name="menuItem">The menu item to build the shape for.</param>
        /// <returns>The menu item shape.</returns>
        public static dynamic BuildLocalMenuItemShape(dynamic shapeFactory, dynamic parentShape, dynamic menu, MenuItem menuItem) {
            var menuItemShape = shapeFactory.LocalMenuItem()
                .Text(menuItem.Text)
                .IdHint(menuItem.IdHint)
                .Href(menuItem.Href)
                .LinkToFirstChild(menuItem.LinkToFirstChild)
                .LocalNav(menuItem.LocalNav)
                .Selected(menuItem.Selected)
                .RouteValues(menuItem.RouteValues)
                .Item(menuItem)
                .Menu(menu)
                .Parent(parentShape);

            foreach (var className in menuItem.Classes)
                menuItemShape.Classes.Add(className);

            return menuItemShape;
        }
    }
}
