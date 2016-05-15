﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;

namespace Orchard.UI.Navigation {
    public class MenuFilter : FilterProvider, IResultFilter {
        private readonly INavigationManager _navigationManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public MenuFilter(INavigationManager navigationManager,
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory) {

            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult)) {
                return;
            }

            WorkContext workContext = _workContextAccessor.GetContext(filterContext);

            const string menuName = "admin";
            if (!AdminFilter.IsApplied(filterContext.RequestContext)) {
                return;
            }

            IEnumerable<MenuItem> menuItems = _navigationManager.BuildMenu(menuName);

            // adding query string parameters
            var routeData = new RouteValueDictionary(filterContext.RouteData.Values);
            var queryString = workContext.HttpContext.Request.QueryString;
            if (queryString != null) {
                foreach (var key in from string key in queryString.Keys where key != null && !routeData.ContainsKey(key) let value = queryString[key] select key) {
                    routeData[key] = queryString[key];
                }
            }

            // Populate main nav
            dynamic menuShape = _shapeFactory.SideMenu().MenuName(menuName);
            NavigationHelper.PopulateMenu(_shapeFactory, menuShape, menuShape, menuItems);

            // Add any know image sets to the main nav
            IEnumerable<string> menuImageSets = _navigationManager.BuildImageSets(menuName);
            if (menuImageSets != null && menuImageSets.Any())
                menuShape.ImageSets(menuImageSets);

            workContext.Layout.SideMenu.Add(menuShape);

            // Populate top nav
            var topMenuName = string.Format("top_{0}", menuName);
            IEnumerable<MenuItem> topMenuItems = _navigationManager.BuildMenu(topMenuName);
            dynamic topMenuShape = _shapeFactory.TopMenu().MenuName(topMenuName);
            NavigationHelper.PopulateMenu(_shapeFactory, topMenuShape, topMenuShape, topMenuItems);

            workContext.Layout.TopMenu.Add(topMenuShape);

            //SIDE MENU
            // Set the currently side menu selected path
            Stack<MenuItem> selectedSideMenuPath = NavigationHelper.SetSelectedPath(menuItems, workContext.HttpContext.Request, routeData) ?? NavigationHelper.SetSelectedPath(topMenuItems, workContext.HttpContext.Request, routeData);

            // Populate local nav related to side menu
            dynamic localSideMenuShape = _shapeFactory.LocalMenu().MenuName(string.Format("local_{0}", menuName));
            NavigationHelper.PopulateLocalMenu(_shapeFactory, localSideMenuShape, localSideMenuShape, selectedSideMenuPath);
            if (localSideMenuShape.Items.Count > 0) { 
                workContext.Layout.LocalNavigation.Add(localSideMenuShape);
            }

            //TOP MENU
            // Set the currently side menu selected path
            Stack<MenuItem> selectedTopMenuPath = NavigationHelper.SetSelectedPath(topMenuItems, workContext.HttpContext.Request, routeData);

            // Populate local nav related to top menu
            dynamic localTopMenuShape = _shapeFactory.LocalMenu().MenuName(string.Format("local_{0}", topMenuName)) ?? NavigationHelper.SetSelectedPath(topMenuItems, workContext.HttpContext.Request, routeData);
            NavigationHelper.PopulateLocalMenu(_shapeFactory, localTopMenuShape, localTopMenuShape, selectedTopMenuPath);
            workContext.Layout.LocalNavigation.Add(localTopMenuShape);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) { }
    }
}