namespace Orchard.Data.Providers {
    public class MySqlStatementProvider : ISqlStatementProvider {
        public string DataProvider {
            get { return "MySql"; }
        }

        public string GetStatement(string command) {
            switch (command) {
                case "random":
                    return "rand()";
                case "table_names":
                    return "select table_name from information_schema.tables where table_schema = DATABASE() and table_type='BASE TABLE'";
            }

            return null;
        }
    }
}
