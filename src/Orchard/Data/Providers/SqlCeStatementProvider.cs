namespace Orchard.Data.Providers {
    public class SqlCeStatementProvider : ISqlStatementProvider {
        public string DataProvider {
            get { return "SqlCe"; }
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
