using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Orchard.MultiTenancy.Annotations {
    public class SqlDatabaseConnectionStringAttribute : ValidationAttribute {
        public override bool IsValid(object value) {
            if (value is string && ((string) value).Length > 0) {
                try {
                    var connectionStringBuilder = new SqlConnectionStringBuilder(value as string);

                    //TODO: (erikpo) Should the keys be checked here to ensure that a valid combination was entered? Needs investigation.
                }
                catch {
                    return false;
                }
            }

            return true;
        }
    }
}