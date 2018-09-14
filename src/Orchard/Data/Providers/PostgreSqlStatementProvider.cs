namespace Orchard.Data.Providers {
    public class PostgreSqlStatementProvider : ISqlStatementProvider {
        public string DataProvider {
            get { return "PostgreSql"; }
        }

        public string GetStatement(string command) {
            switch (command) {
                case "random":
                    return "random()";
                case "table_names":
                    return "select table_name from information_schema.tables where table_schema = 'public' and table_type = 'BASE TABLE'";
            }

            return null;
        }
    }
}
