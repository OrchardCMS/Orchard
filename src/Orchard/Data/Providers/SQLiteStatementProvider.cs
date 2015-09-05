using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Data.Providers
{
    public class SQLiteStatementProvider : ISqlStatementProvider
    {
        public string DataProvider
        {
            get { return "SQLite"; }
        }

        public string GetStatement(string command)
        {
            switch (command)
            {
                case "random":
                    return "rand()";
            }

            return null;
        }
    }
}
