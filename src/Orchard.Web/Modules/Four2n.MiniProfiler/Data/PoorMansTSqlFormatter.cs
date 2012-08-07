// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PoorMansTSqlFormatter.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the PoorMansTSqlFormatter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Data
{
    using PoorMansTSqlFormatterLib;
    using PoorMansTSqlFormatterLib.Formatters;

    using StackExchange.Profiling;
    using StackExchange.Profiling.SqlFormatters;

    public class PoorMansTSqlFormatter : ISqlFormatter
    {

        public string FormatSql(SqlTiming timing)
        {
            var sqlFormatter = new SqlServerFormatter();
            var sqlFormat = sqlFormatter.FormatSql(timing);

            var poorMansFormatter = new TSqlStandardFormatter();
            var fullFormatter = new SqlFormattingManager(poorMansFormatter);
            return fullFormatter.Format(sqlFormat);
        }
    }
}