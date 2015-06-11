using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDeliverable.LicensingService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            ConfigSerializationSettings(config);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ValidateApi/V1",
                routeTemplate: "api/v1/validate/{productId}/{hostname}/{key}",
                defaults: new { controller = "License", action = "Validate" }
            );
        }

        private static void ConfigSerializationSettings(HttpConfiguration config)
        {
            var jsonSetting = new JsonSerializerSettings();
            jsonSetting.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.SerializerSettings = jsonSetting;
        }
    }
}
