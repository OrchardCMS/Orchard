using System;
using System.Data;

namespace Orchard.Data.Migration.Schema {
    public static class SchemaUtils {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Enum.TryParse<System.Data.DbType>(System.String,System.Boolean,System.Data.DbType@)")]
        public static DbType ToDbType(Type type) {
            DbType dbType;
            switch ( Type.GetTypeCode(type) ) {
                case TypeCode.String:
                    dbType = DbType.String;
                    break;
                case TypeCode.Int32:
                    dbType = DbType.Int32;
                    break;
                case TypeCode.DateTime:
                    dbType = DbType.DateTime;
                    break;
                case TypeCode.Boolean:
                    dbType = DbType.Boolean;
                    break;
                default:
                    if(type == typeof(Guid)) 
                        dbType = DbType.Guid;
                    else
                        Enum.TryParse(Type.GetTypeCode(type).ToString(), true, out dbType);
                    break;
            }

            return dbType;
        }

    }
}
