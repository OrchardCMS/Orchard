using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDeliverable.Licensing.Service
{
    internal static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            ConfigSerializationSettings(config);

            config.MapHttpAttributeRoutes();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }

        private static void ConfigSerializationSettings(HttpConfiguration config)
        {
            var jsonSetting = new JsonSerializerSettings();
            jsonSetting.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.SerializerSettings = jsonSetting;
        }
    }
}
