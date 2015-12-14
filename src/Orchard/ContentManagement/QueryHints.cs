using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement
{
    public class QueryHints
    {
        private readonly List<string> _records = new List<string>();
        private static readonly QueryHints _empty = new QueryHints();

        public IEnumerable<string> Records
        {
            get { return _records; }
        }

        public static QueryHints Empty
        {
            get { return _empty; }
        }

        public QueryHints ExpandRecords(IEnumerable<string> records)
        {
            _records.AddRange(records);
            return this;
        }

        public QueryHints ExpandRecords(params string[] records)
        {
            _records.AddRange(records);
            return this;
        }

        public QueryHints ExpandRecords<TRecord1>() where TRecord1 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2, TRecord3>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
            where TRecord3 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name, typeof(TRecord3).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2, TRecord3, TRecord4>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
            where TRecord3 : ContentPartRecord
            where TRecord4 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name, typeof(TRecord3).Name, typeof(TRecord4).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2, TRecord3, TRecord4, TRecord5>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
            where TRecord3 : ContentPartRecord
            where TRecord4 : ContentPartRecord
            where TRecord5 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name, typeof(TRecord3).Name, typeof(TRecord4).Name, typeof(TRecord5).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2, TRecord3, TRecord4, TRecord5, TRecord6>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
            where TRecord3 : ContentPartRecord
            where TRecord4 : ContentPartRecord
            where TRecord5 : ContentPartRecord
            where TRecord6 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name, typeof(TRecord3).Name, typeof(TRecord4).Name, typeof(TRecord5).Name, typeof(TRecord6).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2, TRecord3, TRecord4, TRecord5, TRecord6, TRecord7>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
            where TRecord3 : ContentPartRecord
            where TRecord4 : ContentPartRecord
            where TRecord5 : ContentPartRecord
            where TRecord6 : ContentPartRecord
            where TRecord7 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name, typeof(TRecord3).Name, typeof(TRecord4).Name, typeof(TRecord5).Name, typeof(TRecord6).Name, typeof(TRecord7).Name });
            return this;
        }
        public QueryHints ExpandRecords<TRecord1, TRecord2, TRecord3, TRecord4, TRecord5, TRecord6, TRecord7, TRecord8>()
            where TRecord1 : ContentPartRecord
            where TRecord2 : ContentPartRecord
            where TRecord3 : ContentPartRecord
            where TRecord4 : ContentPartRecord
            where TRecord5 : ContentPartRecord
            where TRecord6 : ContentPartRecord
            where TRecord7 : ContentPartRecord
            where TRecord8 : ContentPartRecord
        {
            _records.AddRange(new[] { typeof(TRecord1).Name, typeof(TRecord2).Name, typeof(TRecord3).Name, typeof(TRecord4).Name, typeof(TRecord5).Name, typeof(TRecord6).Name, typeof(TRecord7).Name, typeof(TRecord8).Name });
            return this;
        }

        public QueryHints ExpandParts<TPart1>() where TPart1 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1));
        }

        public QueryHints ExpandParts<TPart1, TPart2>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2));
        }

        public QueryHints ExpandParts<TPart1, TPart2, TPart3>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
            where TPart3 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2), typeof(TPart3));
        }

        public QueryHints ExpandParts<TPart1, TPart2, TPart3, TPart4>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
            where TPart3 : ContentPart
            where TPart4 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2), typeof(TPart3), typeof(TPart4));
        }

        public QueryHints ExpandParts<TPart1, TPart2, TPart3, TPart4, TPart5>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
            where TPart3 : ContentPart
            where TPart4 : ContentPart
            where TPart5 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2), typeof(TPart3), typeof(TPart4), typeof(TPart5));
        }

        public QueryHints ExpandParts<TPart1, TPart2, TPart3, TPart4, TPart5, TPart6>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
            where TPart3 : ContentPart
            where TPart4 : ContentPart
            where TPart5 : ContentPart
            where TPart6 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2), typeof(TPart3), typeof(TPart4), typeof(TPart5), typeof(TPart6));
        }

        public QueryHints ExpandParts<TPart1, TPart2, TPart3, TPart4, TPart5, TPart6, TPart7>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
            where TPart3 : ContentPart
            where TPart4 : ContentPart
            where TPart5 : ContentPart
            where TPart6 : ContentPart
            where TPart7 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2), typeof(TPart3), typeof(TPart4), typeof(TPart5), typeof(TPart6), typeof(TPart7));
        }

        public QueryHints ExpandParts<TPart1, TPart2, TPart3, TPart4, TPart5, TPart6, TPart7, TPart8>()
            where TPart1 : ContentPart
            where TPart2 : ContentPart
            where TPart3 : ContentPart
            where TPart4 : ContentPart
            where TPart5 : ContentPart
            where TPart6 : ContentPart
            where TPart7 : ContentPart
            where TPart8 : ContentPart
        {
            return ExpandPartsImpl(typeof(TPart1), typeof(TPart2), typeof(TPart3), typeof(TPart4), typeof(TPart5), typeof(TPart6), typeof(TPart7), typeof(TPart8));
        }

        private QueryHints ExpandPartsImpl(params Type[] parts)
        {
            foreach (var part in parts)
            {
                for (var scan = part; scan != typeof(Object); scan = scan.BaseType)
                {
                    if (scan.IsGenericType && scan.GetGenericTypeDefinition() == typeof(ContentPart<>))
                    {
                        _records.Add(scan.GetGenericArguments().Single().Name);
                        break;
                    }
                }
            }
            return this;
        }
    }
}