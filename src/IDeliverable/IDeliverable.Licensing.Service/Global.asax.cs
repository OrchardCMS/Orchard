using System;
using System.Configuration;
using System.Web.Http;
using Microsoft.ApplicationInsights.Extensibility;

namespace IDeliverable.Licensing.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Configure ApplicationInsights instrumentation key.
            var instrumentationKey = ConfigurationManager.AppSettings["ApplicationInsights.InstrumentationKey"];
            if (!String.IsNullOrWhiteSpace(instrumentationKey))
                TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            else
                TelemetryConfiguration.Active.DisableTelemetry = true;
        }
    }
}
