using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Utils;
using NHibernate.Mapping;

namespace Orchard.Data.Providers
{
    public class SQLiteDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _fileName;
        private readonly string _dataFolder;


        public SQLiteDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _fileName = Path.Combine(dataFolder, "Orchard.sqlite");
        }

        public SQLiteDataServicesProvider(string fileName) {
            _dataFolder = Path.GetDirectoryName(fileName);
            _fileName = fileName;
           
        }

        public static string ProviderName {
            get { return "SQLite"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            SQLiteConfiguration persistence = SQLiteConfiguration.Standard;

            if (createDatabase) {
                if (File.Exists(_fileName))
                    File.Delete(_fileName);
            }

            string localConnectionString = string.Format("Data Source='{0}'",
                _fileName);

            if (!File.Exists(_fileName) && !string.IsNullOrEmpty(_dataFolder)) {
                Directory.CreateDirectory(_dataFolder);
            }

            persistence = persistence.ConnectionString(localConnectionString);
            return persistence;
        }
    }
}
