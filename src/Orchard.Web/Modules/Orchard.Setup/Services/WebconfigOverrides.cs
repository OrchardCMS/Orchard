using System.Configuration;

namespace Orchard.Setup.Services
{

    public class WebconfigOverrides
    {
        private const string AppSettingDefaultDataProvider = "defaultProvider";
        private const string AppSettingDefaultRecipe = "defaultRecipe";
        private const string ConnectionStringDefaultConnectionString = "defaultConnection";

        public string DataProvider { get; set; }
        public string DataConnectionString { get; set; }
        public string DefaultRecipe { get; set; }

        public static WebconfigOverrides Load()
        {
            var result = new WebconfigOverrides();

            if (ConfigurationManager.ConnectionStrings[ConnectionStringDefaultConnectionString] != null)
            {
                result.DataConnectionString = ConfigurationManager.ConnectionStrings[ConnectionStringDefaultConnectionString].ConnectionString;
                result.DataProvider = "SqlServer";
            }

            if (ConfigurationManager.AppSettings[AppSettingDefaultDataProvider] != null)
            {
                result.DataProvider = ConfigurationManager.AppSettings[AppSettingDefaultDataProvider];
            }

            if (ConfigurationManager.AppSettings[AppSettingDefaultRecipe] != null)
            {
                result.DefaultRecipe = ConfigurationManager.AppSettings[AppSettingDefaultRecipe];
            }

            return result;
        }
    }
}