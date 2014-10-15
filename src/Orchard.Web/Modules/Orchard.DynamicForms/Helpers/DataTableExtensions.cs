using System;
using System.Data;
using System.IO;
using System.Linq;

namespace Orchard.DynamicForms.Helpers {
    public static class DataTableExtensions {
        public static string ToCsv(this DataTable table, string delimiter = ",", bool includeHeader = true) {
            using (var writer = new StringWriter()) {
                ToCsv(table, writer, delimiter, includeHeader);
                return writer.GetStringBuilder().ToString();
            }
        }

        public static Stream ToCsv(this DataTable table, Stream output, string delimiter = ",", bool includeHeader = true) {
            var writer = new StreamWriter(output);
            ToCsv(table, writer, delimiter, includeHeader);
            return output;
        }

        public static TextWriter ToCsv(this DataTable table, TextWriter writer, string delimiter = ",", bool includeHeader = true) {
            if (includeHeader) {
                var columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                writer.WriteLine(String.Join(",", columnNames));
            }

            var projection =
                from DataRow row in table.Rows
                select row.ItemArray.Select(field => String.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));

            foreach (var fields in projection) {
                writer.WriteLine(String.Join(",", fields));
            }

            writer.Flush();
            return writer;
        }
    }
}