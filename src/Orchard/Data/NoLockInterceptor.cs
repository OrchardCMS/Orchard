using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.SqlCommand;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;

namespace Orchard.Data {
    public class NoLockInterceptor : EmptyInterceptor, ISessionInterceptor {

        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<INoLockTableProvider> _noLockTableProviders;

        public NoLockInterceptor(
            ShellSettings shellSettings,
            IEnumerable<INoLockTableProvider> noLockTableProviders) {

            _shellSettings = shellSettings;
            _noLockTableProviders = noLockTableProviders;
        }

        private List<string> _tableNames;
        public List<string> TableNames {
            get {
                if (_tableNames == null) {
                    _tableNames = new List<string>(
                        _noLockTableProviders
                            .SelectMany(nltp => nltp.GetTableNames())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Select(n => GetPrefixedTableName(n.Trim())));
                }
                return _tableNames;
            }
        }

        private string GetPrefixedTableName(string tableName) {
            if (string.IsNullOrWhiteSpace(_shellSettings.DataTablePrefix)) {
                return tableName;
            }

            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
        
        // based on https://stackoverflow.com/a/39518098/2669614
        public override SqlString OnPrepareStatement(SqlString sql) {
            // only work on select queries
            if (sql.StartsWithCaseInsensitive("select")) {
                // see whether we have anything to add the NOLOCK hint to
                var tableNamesForQuery =
                    TableNames.Where(tn => sql.IndexOfCaseInsensitive(tn) >= 0);
                if (tableNamesForQuery.Any()) {
                    // Modify the sql to add hints
                    // the sql may contain substrings, and we want to also process them
                    var sqlString = sql.ToString();
                    // https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions?redirectedfrom=MSDN#balancing-group-definitions
                    const string regexPattern = @"^[^\(\)]*(((?'Open'\()[^\(\)]*)+((?'Close-Open'\))[^\(\)]*)+)*(?(Open)(?!))$";
                    var matches = Regex.Match(sqlString, regexPattern, RegexOptions.Compiled);
                    // matches.Groups["Close"] contains all portions of sqlString that are between
                    // opening and closing parentheses. The captured strings in that group already
                    // don't have the starting and ending parentheses.
                    // Each one of those may be a query, that may have its own subqueries. Those
                    // subqueries are already included in the captures here.
                    // The captures in this group do not include the full query we are processing.
                    // The captured strings in this group are ordered with respect to where they are
                    // found in the original string: that means that each string may be contained
                    // in any, all or none of the following ones in the array. When a captured string
                    // is part of another, it is so in its entirety, meaning there cannote be a case
                    // when two captured strings intersect partially.
                    // Take for example the following query:
                    //
                    // select count(A.Id) as myResult
                    // from
                    //    Table1 A
                    // where
                    //    A.Id in (
                    //        select
                    //            distinct T1.Id
                    //        from
                    //            Table1 T1
                    //            inner join Table2 T2 on T1.FK1 = T2.Id
                    //            inner join Table3 T3 on T2.FK2 = T3.Id
                    //        where
                    //            (T3.Name in ('example')) and T1.Published = 1)
                    //
                    // The elements of matches.Groups["Close"].Captures would be, in order, the
                    // following strings:
                    //   A.Id
                    //   'example'
                    //   T3.Name in ('example')
                    //   select distinct T1.Id from Table1 T1 inner join Table2 T2 on T1.FK1 = T2.Id inner join Table3 T3 on T2.FK2 = T3.Id where (T3.Name in ('example')) and T1.Published = 1
                    //
                    // The first element is in none of the others.
                    // The second string is included in both of the following ones.
                    // The third string is included in the last.
                    // Naturally, all the strings are included in the original query.
                    // If we alter any of these substrings, we need to make sure the change is
                    // not overwritten by changing the next one. We also need to make sure we are
                    // not overwriting portions of the query we should not be changing. In the example,
                    // the first captured string is the one that corresponds to the parameter of the
                    // count statement, but not to the A.Id that is in the first where statement.
                    // The Capture objects have the index the string they represent has in the original
                    // string.
                    // We should also note that we are only going to alter the substrings that represent
                    // a subquery of the original one. We need to be careful of the fact that we may
                    // have multiple and even nested subqueries.

                    // We are only interested in those Captures that involve the tables we are affecting:
                    var affectedCaptures = new List<CaptureWrapper>();
                    // we are going to assign to each CaptureWrapper an identifier that we make sure that
                    // isn't in the query
                    int tag = 0;
                    const string tagBase = @"@sub{0}bus@";
                    for (int i = 0; i < matches.Groups["Close"].Captures.Count; i++) {
                        // for loop because CaptureCollection does not play nice with iterators
                        var cap = matches.Groups["Close"].Captures[i];
                        var tablesHere = tableNamesForQuery
                            .Where(tn => cap.Value.IndexOf(tn, StringComparison.InvariantCultureIgnoreCase) >= 0);
                        if (tablesHere.Any()) {
                            var currentTag = string.Format(tagBase, tag++);
                            while (sqlString.IndexOf(currentTag) >= 0) {
                                currentTag = string.Format(tagBase, tag++);
                            }
                            affectedCaptures.Add(new CaptureWrapper(cap, tablesHere) { Tag = currentTag });
                        }
                    }
                    // matches.Groups[0].Captures[0] is the original string
                    affectedCaptures.Add(new CaptureWrapper(matches.Groups[0].Captures[0], tableNamesForQuery));
                    // start processing the substrings. Use for-loops to nest them
                    for (int i = 0; i < affectedCaptures.Count; i++) {
                        var inner = affectedCaptures[i];
                        inner.AddNoLockHints();
                        if (inner.IsAltered) {
                            // replace the newly altered substring in the first string that contains it
                            for (int j = i + 1; j < affectedCaptures.Count; j++) {
                                // since we are processing the left-most substring first, we will have to
                                // find where we are replacing a string starting from the end of the one
                                // we will write into.
                                var outer = affectedCaptures[j];
                                if (inner.OriginalIndex >= outer.OriginalIndex
                                    && inner.OriginalIndex <= outer.OriginalEnd) {
                                    // inner is inside outer
                                    var insertionIndex = inner.OriginalIndex - outer.OriginalIndex;
                                    if (outer.IsAltered) {
                                        // outer has already been changed by previous changes to subqueries
                                        // so we should compute the start index for the current substring
                                        // starting from the end.
                                        var oFromEnd = outer.OriginalEnd - inner.OriginalIndex;
                                        insertionIndex = outer.Value.Length - oFromEnd;
                                    } else {
                                        // outer is still the same as what we captured originally
                                        // we are changing it here for the first time.
                                        // This means the index of our ssubstring within it has not changed
                                    }
                                    outer.Value = outer.Value
                                        // Remove old substring
                                        .Remove(insertionIndex, inner.OriginalLength)
                                        // insert the tag that we will replace with the new substring
                                        .Insert(insertionIndex, inner.Tag);
                                    outer.IsAltered = true;
                                    // alterations will cascade by the outer loop, so here we stop
                                    // propagating them after the first
                                    break;
                                }
                            }
                        }
                    }
                    // rebuild query
                    for(int i = 0; i < affectedCaptures.Count; i++) {
                        var inner = affectedCaptures[i];
                        for (int j = i + 1; j < affectedCaptures.Count; j++) {
                            var outer = affectedCaptures[j];
                            outer.Value = outer.Value
                                .Replace(inner.Tag, inner.Value);
                        }
                    }
                    sql = SqlString.Parse(affectedCaptures.Last().Value);
                }
            }

            return sql;
        }

        class CaptureWrapper {
            private Capture Source { get; set; }
            public CaptureWrapper(Capture source, IEnumerable<string> tableNames) {
                Source = source;
                Value = OriginalValue;
                TableNames = tableNames;
            }

            public int OriginalIndex { get { return Source.Index; } }
            public int OriginalLength { get { return Source.Length; } }
            public int OriginalEnd { get { return OriginalIndex + OriginalLength; } }
            public string OriginalValue { get { return Source.Value; } }
            public string Value { get; set; }

            public IEnumerable<string> TableNames { get; set; }

            public string Tag { get; set; }

            public bool IsAltered { get; set; }

            public void AddNoLockHints() {
                Value = AddNoLockHints(Value, TableNames); 
            }

            private string AddNoLockHints(string query, IEnumerable<string> tableNames) {
                var trimmed = query.Trim();
                if (trimmed.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)
                    && trimmed.Length > 6
                    && Char.IsWhiteSpace(trimmed, 6)) {
                    // this fails to parse subqueries, meaning it will not apply the NOLOCK
                    // hint to the tables within them
                    var parts = query.ToString().Split().ToList();
                    var fromItem = parts.FirstOrDefault(p => p.Trim().Equals("from", StringComparison.OrdinalIgnoreCase));
                    int fromIndex = fromItem != null ? parts.IndexOf(fromItem) : -1;

                    if (fromIndex == -1)
                        return query;

                    var whereItem = parts.FirstOrDefault(p => p.Trim().Equals("where", StringComparison.OrdinalIgnoreCase));
                    int whereIndex = whereItem != null ? parts.IndexOf(whereItem) : parts.Count;

                    foreach (var tableName in tableNames) {
                        // set NOLOCK for each one of these tables
                        var tableItem = parts
                            .FirstOrDefault(p => p.Trim()
                                .Equals(tableName, StringComparison.OrdinalIgnoreCase));
                        if (tableItem != null) {
                            // the table is involved in this statement
                            var tableIndex = parts.IndexOf(tableItem);
                            // recompute whereIndex in case we added stuff to parts
                            whereIndex = whereItem != null ? parts.IndexOf(whereItem) : parts.Count;
                            if (tableIndex > fromIndex && tableIndex < whereIndex) { // sanity check
                                                                                     // if before the table name we have "," or "FROM", this is not a join, but rather
                                                                                     // something like "FROM tableName alias ..."
                                                                                     // we can insert "WITH(NOLOCK)" after that
                                if (tableIndex == fromIndex + 1
                                    || parts[tableIndex - 1].Equals(",")) {

                                    parts.Insert(tableIndex + 2, "WITH(NOLOCK)");
                                } else {
                                    // probably doing a join, so edit the next "on" and make it
                                    // "WITH (NOLOCK) on"
                                    for (int i = tableIndex + 1; i < whereIndex; i++) {
                                        if (parts[i].Trim().Equals("WITH(NOLOCK)", StringComparison.OrdinalIgnoreCase)) {
                                            // we processed this table anme already
                                            break;
                                        }
                                        if (parts[i].Trim().Equals("on", StringComparison.OrdinalIgnoreCase)) {
                                            parts[i] = "WITH(NOLOCK) on";
                                            break;
                                        }
                                    }
                                }
                                IsAltered = true;
                            }
                        }
                    }

                    query = string.Join(" ", parts);
                }
                return query;
            }
        }

    }
}
