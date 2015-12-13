namespace Orchard.Data.Providers {
    public class SqlServerStatementProvider : ISqlStatementProvider {
        public string DataProvider {
            get { return "SqlServer"; }
        }

        public string GetStatement(string command) {
            switch (command) {
                case "random":
                    return "newid()";
            }

            return null;
        }
    }
}
