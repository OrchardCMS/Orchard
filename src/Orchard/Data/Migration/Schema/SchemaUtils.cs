using System;
using System.Data;

namespace Orchard.Data.Migration.Schema {
    public static class SchemaUtils {
        public static DbType ToDbType(Type type) {
            DbType dbType;
            switch ( System.Type.GetTypeCode(type) ) {
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
                    Enum.TryParse(Type.GetTypeCode(type).ToString(), true, out dbType);
                    break;
            }

            return dbType;
        }

    }
}
