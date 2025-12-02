using System;
using System.Web.Mvc;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using Orchard.UI.Notify;

namespace Orchard.MessageBus.Services
{
    /// <summary>
    /// Prevents the SQL Server Service Broker feature from being enabled for tenants that are not using
    /// Microsoft SQL Server as their database provider.
    /// </summary>
    /// <remarks>
    /// The implementation is hackish but there seems to be no other way: if it would use <see cref="Orchard.UI.Notify.INotifier"/>
    /// it wouldn't work since <see cref="Orchard.UI.Notify.NotifyFilter"/> that writes out notifications to TempData 
    /// runs before feature events are raised. Thus we have to manually add the messages to TempData, just as the filter 
    /// would do.
    /// </remarks>
    public class SqlServerServiceBrokerFeatureGuard : FilterProvider, IFeatureEventHandler, IActionFilter
    {
        private const string FeatureId = "Orchard.MessageBus.SqlServerServiceBroker";
        private const string TempDataKey = FeatureId + ".TempData";

        private readonly ShellSettings _shellSettings;
        private readonly IFeatureManager _featureManager;
        private readonly IHttpContextAccessor _hca;

        public Localizer T { get; set; }

        public SqlServerServiceBrokerFeatureGuard(
            ShellSettings shellSettings,
            IFeatureManager featureManager,
            IHttpContextAccessor hca)
        {
            _shellSettings = shellSettings;
            _featureManager = featureManager;
            _hca = hca;

            T = NullLocalizer.Instance;
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext) { }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {
            var httpContext = _hca.Current();

            if (httpContext == null) return;

            httpContext.Items[TempDataKey] = filterContext.Controller.TempData;
        }

        public void Installing(Feature feature) { }

        public void Installed(Feature feature) { }

        public void Enabling(Feature feature) { }

        public void Enabled(Feature feature)
        {
            if (feature.Descriptor.Id != FeatureId)
                return;

            if (!_shellSettings.DataProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                _featureManager.DisableFeatures(new[] { FeatureId }, force: true);

                AddNotification(
                    T("The SQL Server Service Broker cannot be enabled because it requires Microsoft SQL Server."),
                    NotifyType.Error);
            }
        }

        public void Disabling(Feature feature) { }

        public void Disabled(Feature feature) { }

        public void Uninstalling(Feature feature) { }


        public void Uninstalled(Feature feature) { }

        private void AddNotification(LocalizedString message, NotifyType notifyType = NotifyType.Warning)
        {
            var tempDataDictionary = _hca.Current()?.Items[TempDataKey];

            if (tempDataDictionary == null) return;

            ((TempDataDictionary)tempDataDictionary)["messages"] +=
                notifyType.ToString() +
                ":" +
                message.Text +
                System.Environment.NewLine +
                "-" +
                System.Environment.NewLine;
        }
    }
}
