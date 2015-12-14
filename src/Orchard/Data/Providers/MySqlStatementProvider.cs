namespace Orchard.Data.Providers {
    public class MySqlStatementProvider : ISqlStatementProvider {
        public string DataProvider {
            get { return "MySql"; }
        }

        public string GetStatement(string command) {
            switch (command) {
                case "random":
                    return "rand()";
            }

            return null;
        }
    }
}
