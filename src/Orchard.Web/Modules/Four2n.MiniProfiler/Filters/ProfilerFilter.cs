// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfilerFilter.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Filter for injecting profiler view code.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using global::Orchard;
    using global::Orchard.DisplayManagement;
    using global::Orchard.Mvc.Filters;
    using global::Orchard.Security;
    using global::Orchard.UI.Admin;
    using Four2n.Orchard.MiniProfiler.Services;

    using StackExchange.Profiling;

    /// <summary>
    /// Filter for injecting profiler view code.
    /// </summary>
    public class ProfilerFilter : FilterProvider, IResultFilter, IActionFilter
    {
        #region Constants and Fields

        private readonly IAuthorizer authorizer;
        private readonly dynamic shapeFactory;

        private readonly WorkContext workContext;

        private readonly IProfilerService profiler;

        #endregion

        #region Constructors and Destructors

        public ProfilerFilter(WorkContext workContext, IAuthorizer authorizer, IShapeFactory shapeFactory, IProfilerService profiler)
        {
            this.workContext = workContext;
            this.shapeFactory = shapeFactory;
            this.authorizer = authorizer;
            this.profiler = profiler;
        }

        #endregion

        #region Public Methods

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            this.profiler.StepStop(StepKeys.ActionFilter);
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var tokens = filterContext.RouteData.DataTokens;
            string area = tokens.ContainsKey("area") && !string.IsNullOrEmpty(tokens["area"].ToString()) ?
                string.Concat(tokens["area"], ".") :
                string.Empty;
            string controller = string.Concat(filterContext.Controller.ToString().Split('.').Last(), ".");
            string action = filterContext.ActionDescriptor.ActionName;
            this.profiler.StepStart(StepKeys.ActionFilter, "Controller: " + area + controller + action);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            if (!this.IsActivable())
            {
                return;
            }

            this.profiler.StepStop(StepKeys.ResultFilter);
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            if (!this.IsActivable())
            {
                return;
            }

            var place = this.workContext.Layout.Footer ?? this.workContext.Layout.Head;
            place.Add(this.shapeFactory.MiniProfilerTemplate());

            this.profiler.StepStart(StepKeys.ResultFilter, string.Format("Result: {0}", filterContext.Result));
        }

        #endregion

        #region Methods

        private bool IsActivable()
        {
            // activate on front-end only
            if (AdminFilter.IsApplied(new RequestContext(this.workContext.HttpContext, new RouteData())))
            {
                return false;
            }

            // if not logged as a site owner, still activate if it's a local request (development machine)
            if (!this.authorizer.Authorize(StandardPermissions.SiteOwner))
            {
                return this.workContext.HttpContext.Request.IsLocal;
            }

            return true;
        }

        #endregion
    }
}