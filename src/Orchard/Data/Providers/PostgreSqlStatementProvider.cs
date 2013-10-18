namespace Orchard.Data.Providers {
    public class PostgreSqlStatementProvider : ISqlStatementProvider {
        public string DataProvider {
            get { return "PostgreSql"; }
        }

        public string GetStatement(string command) {
            switch (command) {
                case "random":
                    return "random()";
            }

            return null;
        }
    }
}
