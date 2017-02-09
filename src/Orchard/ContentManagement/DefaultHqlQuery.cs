using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Transform;
using Orchard.ContentManagement.Records;
using Orchard.Data.Providers;
using Orchard.Environment.Configuration;
using Orchard.Utility.Extensions;

namespace Orchard.ContentManagement {

    public class DefaultHqlQuery : IHqlQuery {
        private readonly ISession _session;
        private readonly IEnumerable<ISqlStatementProvider> _sqlStatementProviders;
        private readonly ShellSettings _shellSettings;
        private VersionOptions _versionOptions;
        private string[] _includedPartRecords = new string[0];
        private bool _cacheable;

        protected IJoin _from;
        protected readonly List<Tuple<IAlias, Join>> _joins = new List<Tuple<IAlias, Join>>();
        protected readonly List<Tuple<IAlias, Action<IHqlExpressionFactory>>> _wheres = new List<Tuple<IAlias, Action<IHqlExpressionFactory>>>();
        protected readonly List<Tuple<IAlias, Action<IHqlSortFactory>>> _sortings = new List<Tuple<IAlias, Action<IHqlSortFactory>>>();

        public IContentManager ContentManager { get; private set; }

        public DefaultHqlQuery(
            IContentManager contentManager, 
            ISession session,
            IEnumerable<ISqlStatementProvider> sqlStatementProviders,
            ShellSettings shellSettings) {
            _session = session;
            _sqlStatementProviders = sqlStatementProviders;
            _shellSettings = shellSettings;
            ContentManager = contentManager;
        }

        internal string PathToAlias(string path) {
            if (String.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException("Path can't be empty");
            }

            return Char.ToLower(path[0], CultureInfo.InvariantCulture) + path.Substring(1);
        }

        internal Join BindNamedAlias(string alias) {
            var tuple = _joins.FirstOrDefault(x => x.Item2.Name == alias);
            return tuple == null ? null : tuple.Item2;
        }

        internal IAlias BindCriteriaByPath(IAlias alias, string path, string type = null, Action<IHqlExpressionFactory> withPredicate = null) {
            return BindCriteriaByAlias(alias, path, PathToAlias(path), type, withPredicate);
        }

        internal IAlias BindCriteriaByAlias(IAlias alias, string path, string aliasName, string type = null, Action<IHqlExpressionFactory> withPredicate = null) {
            // is this Join already existing (based on aliasName)

            Join join = BindNamedAlias(aliasName);

            if (join == null) {
                join = new Join(path, aliasName, type, withPredicate);
                _joins.Add(new Tuple<IAlias, Join>(alias, join));
            }

            return join;
        }

        internal IAlias BindTypeCriteria() {
            // ([ContentItemVersionRecord] >join> [ContentItemRecord]) >join> [ContentType]
            return BindCriteriaByAlias(BindItemCriteria(), "ContentType", "ct");
        }

        internal IAlias BindItemCriteria(string type = "") {
            // [ContentItemVersionRecord] >join> [ContentItemRecord]
            return BindCriteriaByAlias(BindItemVersionCriteria(type), typeof(ContentItemRecord).Name, "ci");
        }

        internal IAlias BindItemVersionCriteria(string type = "") {
            _from = _from ?? new Join(typeof(ContentItemVersionRecord).FullName, "civ", type);
            if (_from.Type.Length < type.Length) {
                _from.Type = type;
            }

            return _from;
        }

        internal IAlias BindPartCriteria<TRecord>(string type = null, Action<IHqlExpressionFactory> withPredicate = null) where TRecord : ContentPartRecord {
            return BindPartCriteria(typeof(TRecord), type, withPredicate);
        }

        internal IAlias BindPartCriteria(Type contentPartRecordType, string type = null, Action<IHqlExpressionFactory> withPredicate = null) {
            if (!contentPartRecordType.IsSubclassOf(typeof(ContentPartRecord))) {
                throw new ArgumentException("The type must inherit from ContentPartRecord", "contentPartRecordType");
            }

            if (contentPartRecordType.IsSubclassOf(typeof(ContentPartVersionRecord))) {
                return BindCriteriaByPath(BindItemVersionCriteria(), contentPartRecordType.Name, type, withPredicate);
            }
            return BindCriteriaByPath(BindItemCriteria(), contentPartRecordType.Name, type, withPredicate);
        }

        internal void Where(IAlias alias, Action<IHqlExpressionFactory> predicate) {
            _wheres.Add(new Tuple<IAlias, Action<IHqlExpressionFactory>>(alias, predicate));
        }
        
        internal IAlias ApplyHqlVersionOptionsRestrictions(VersionOptions versionOptions) {
            var alias = BindItemVersionCriteria();

            if (versionOptions == null) {
                Where(alias, x => x.Eq("Published", true));
            }
            else if (versionOptions.IsPublished) {
                Where(alias, x => x.Eq("Published", true));
            }
            else if (versionOptions.IsLatest) {
                Where(alias, x => x.Eq("Latest", true));
            }
            else if (versionOptions.IsDraft) {
                Where(alias, x => x.And(y => y.Eq("Latest", true), y => y.Eq("Published", false)));
            }
            else if (versionOptions.IsAllVersions) {
                // no-op... all versions will be returned by default
            }
            else {
                throw new ApplicationException("Invalid VersionOptions for content query");
            }

            return alias;
        }

        public IHqlQuery Join(Action<IAliasFactory> alias) {
            var aliasFactory = new DefaultAliasFactory(this);
            alias(aliasFactory);
            return this;
        }

        public IHqlQuery Where(Action<IAliasFactory> alias, Action<IHqlExpressionFactory> predicate) {
            var aliasFactory = new DefaultAliasFactory(this);
            alias(aliasFactory);
            Where(aliasFactory.Current, predicate);
            return this;
        }

        public IHqlQuery OrderBy(Action<IAliasFactory> alias, Action<IHqlSortFactory> order) {
            var aliasFactory = new DefaultAliasFactory(this);
            alias(aliasFactory);

            _sortings.Add(new Tuple<IAlias, Action<IHqlSortFactory>>(aliasFactory.Current, order));
            return this;
        }

        public IHqlQuery ForType(params string[] contentTypeNames) {
            if (contentTypeNames != null && contentTypeNames.Length != 0) {
                Where(BindTypeCriteria(), x => x.InG("Name", contentTypeNames));
            }
            
            return this;
        }

        public IHqlQuery Include(params string[] contentPartRecords) {
            _includedPartRecords = _includedPartRecords.Union(contentPartRecords).ToArray();
            return this;
        }

        public IHqlQuery ForVersion(VersionOptions options) {
            _versionOptions = options;
            return this;
        }

        public IHqlQuery<T> ForPart<T>() where T : IContent {
            return new DefaultHqlQuery<T>(this);
        }

        public IEnumerable<ContentItem> List() {
            return Slice(0, 0);
        }

        public IEnumerable<ContentItem> Slice(int skip, int count) {
            ApplyHqlVersionOptionsRestrictions(_versionOptions);
            _cacheable = true;
            
            var hql = ToHql(false);

            var query = _session
                .CreateQuery(hql)
                .SetCacheable(_cacheable)
                ;

            if (skip != 0) {
                query.SetFirstResult(skip);
            }
            if (count != 0 && count != Int32.MaxValue) {
                query.SetMaxResults(count);
            }

            var ids = query
                .SetResultTransformer(Transformers.AliasToEntityMap)
                .List<IDictionary>()
                .Select(x => (int)x["Id"]);

            return ContentManager.GetManyByVersionId(ids, new QueryHints().ExpandRecords(_includedPartRecords));
        }

        public int Count() {
            ApplyHqlVersionOptionsRestrictions(_versionOptions);
            var hql = ToHql(true);
            hql = "select count(Id) from Orchard.ContentManagement.Records.ContentItemVersionRecord where Id in ( " + hql + " )";
            return Convert.ToInt32(_session.CreateQuery(hql)
                           .SetCacheable(true)
                           .UniqueResult())
                ;
        }

        public string ToHql(bool count) {
            var sb = new StringBuilder();

            if (count) {
                sb.Append("select distinct civ.Id as Id").AppendLine();
            }
            else {
                sb.Append("select distinct civ.Id as Id");

                // add sort properties in the select
                foreach (var sort in _sortings) {
                    var sortFactory = new DefaultHqlSortFactory();
                    sort.Item2(sortFactory);

                    if (!sortFactory.Randomize) {
                        sb.Append(", ");
                        sb.Append(sort.Item1.Name).Append(".").Append(sortFactory.PropertyName);
                    }
                    else {
                        // select distinct can't be used with newid()
                        _cacheable = false;
                        sb.Replace("select distinct", "select ");
                    }
                }

                sb.AppendLine();
            }

            sb.Append("from ").Append(_from.TableName).Append(" as ").Append(_from.Name).AppendLine();

            foreach (var join in _joins) {
                sb.Append(join.Item2.Type + " " +
                    join.Item1.Name + "." + join.Item2.TableName +
                    " as " + join.Item2.Name);
                if (join.Item2.WithPredicate != null) {
                    var predicate = join.Item2.WithPredicate;
                    var expressionFactory = new DefaultHqlExpressionFactory();
                    predicate(expressionFactory);
                    sb.Append(" with " + expressionFactory.Criterion.ToHql(join.Item2));
                }
                sb.AppendLine();
            }

            // generating where clause
            if (_wheres.Any()) {
                sb.Append("where ");

                var expressions = new List<string>();

                foreach (var where in _wheres) {
                    var expressionFactory = new DefaultHqlExpressionFactory();
                    where.Item2(expressionFactory);
                    expressions.Add(expressionFactory.Criterion.ToHql(where.Item1));
                }

                sb.Append("(").Append(String.Join(") AND (", expressions.ToArray())).Append(")").AppendLine();
            }

            // generating order by clause
            bool firstSort = true;
            foreach (var sort in _sortings) {
                if (!firstSort) {
                    sb.Append(", ");
                }
                else {
                    sb.Append("order by ");
                    firstSort = false;
                }

                var sortFactory = new DefaultHqlSortFactory();
                sort.Item2(sortFactory);

                if (sortFactory.Randomize) {

                    string command = null;

                    foreach (var sqlStatementProvider in _sqlStatementProviders) {
                        if (!String.Equals(sqlStatementProvider.DataProvider, _shellSettings.DataProvider)) {
                            continue;
                        }

                        command = sqlStatementProvider.GetStatement("random") ?? command;
                    }

                    if (command != null) {
                        sb.Append(command);    
                    }
                }
                else {
                    sb.Append(sort.Item1.Name).Append(".").Append(sortFactory.PropertyName);
                    if (!sortFactory.Ascending) {
                        sb.Append(" desc");
                    }
                }
            }

            // no order clause was specified, use a default sort order, unless it's a count
            // query hence it doesn't need one
            if (firstSort && !count) {
                sb.Append("order by civ.Id");
            }

            return sb.ToString();
        }

    }

    public class DefaultHqlQuery<TPart> : IHqlQuery<TPart> where TPart : IContent {
        private readonly DefaultHqlQuery _query;

        public DefaultHqlQuery(DefaultHqlQuery query) {
            _query = query;
        }

        public IContentManager ContentManager {
            get { return _query.ContentManager; }
        }

        public IHqlQuery<TPart> ForType(params string[] contentTypes) {
            _query.ForType(contentTypes);
            return new DefaultHqlQuery<TPart>(_query);
        }

        public IHqlQuery<TPart> ForVersion(VersionOptions options) {
            _query.ForVersion(options);
            return new DefaultHqlQuery<TPart>(_query);
        }

        IEnumerable<TPart> IHqlQuery<TPart>.List() {
            return _query.List().AsPart<TPart>();
        }

        IEnumerable<TPart> IHqlQuery<TPart>.Slice(int skip, int count) {
            return _query.Slice(skip, count).AsPart<TPart>();
        }

        int IHqlQuery<TPart>.Count() {
            return _query.Count();
        }

        public IHqlQuery<TPart> Join(Action<IAliasFactory> alias) {
            _query.Join(alias);
            return new DefaultHqlQuery<TPart>(_query);
        }

        public IHqlQuery<TPart> Where(Action<IAliasFactory> alias, Action<IHqlExpressionFactory> predicate) {
            _query.Where(alias, predicate);
            return new DefaultHqlQuery<TPart>(_query);
        }

        public IHqlQuery<TPart> OrderBy(Action<IAliasFactory> alias, Action<IHqlSortFactory> order) {
            _query.OrderBy(alias, order);
            return new DefaultHqlQuery<TPart>(_query);
        }
    }

    public class Alias : IAlias {
        public Alias(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException("Alias can't be empty");
            }
            
            Name = name.Strip('-');
        }

        public DefaultHqlQuery<IContent> Query { get; set; }
        public string Name { get; set; }
    }

    public interface IJoin : IAlias {
        string TableName { get; set; }
        string Type { get; set; }
        Action<IHqlExpressionFactory> WithPredicate { get; set; }
    }

    public class Sort {

        public Sort(IAlias alias, string  propertyName, bool ascending) {
            Alias = alias;
            PropertyName = propertyName;
            Ascending = ascending;
        }

        public IAlias Alias { get; set; }
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }
    }

    public class Join : Alias, IJoin {

        public Join(string tableName, string alias)
            : this(tableName, alias, "join", null) {}

        public Join(string tableName, string alias, string type)
            : this(tableName, alias, type, null) {
        }

        public Join(string tableName, string alias, string type, Action<IHqlExpressionFactory> withPredicate)
            : base(alias) {
            if (String.IsNullOrEmpty(tableName)) {
                throw new ArgumentException("Table Name can't be empty");
            }

            TableName = tableName;
            Type = type ?? "join";
            WithPredicate = withPredicate;
        }

        public string TableName { get; set; }
        public string Type { get; set; }
        public Action<IHqlExpressionFactory> WithPredicate { get; set; }
    }

    public class DefaultHqlSortFactory : IHqlSortFactory
    {
        public bool Ascending { get; set; }
        public string PropertyName { get; set; }
        public bool Randomize { get; set; }

        public void Asc(string propertyName) {
            PropertyName = propertyName;
            Ascending = true;
        }

        public void Desc(string propertyName) {
            PropertyName = propertyName;
            Ascending = false;
        }

        public void Random() {
            Randomize = true;
        }
    }

    public class DefaultAliasFactory : IAliasFactory{
        private readonly DefaultHqlQuery _query;
        public IAlias Current { get; private set; }

        public DefaultAliasFactory(DefaultHqlQuery query) {
            _query = query;
            Current = _query.BindItemCriteria();
        }

        public IAliasFactory ContentPartRecord<TRecord>(string type = null, Action<IHqlExpressionFactory> withPredicate = null) where TRecord : ContentPartRecord {
            Current = _query.BindPartCriteria<TRecord>(type, withPredicate);
            return this;
        }

        public IAliasFactory ContentPartRecord(Type contentPartRecord, string type = null, Action<IHqlExpressionFactory> withPredicate = null) {
            if(!contentPartRecord.IsSubclassOf(typeof(ContentPartRecord))) {
                throw new ArgumentException("Type must inherit from ContentPartRecord", "contentPartRecord");
            }

            Current = _query.BindPartCriteria(contentPartRecord, type, withPredicate);
            return this;
        }

        public IAliasFactory Property(string propertyName, string alias) {
            Current = _query.BindCriteriaByAlias(Current, propertyName, alias);
            return this;
        }

        public IAliasFactory Named(string alias) {
            Current = _query.BindNamedAlias(alias);
            return this;
        }

        public IAliasFactory ContentItem() {
            return Named("ci");
        }

        public IAliasFactory ContentItemVersion() {
            Current = _query.BindItemVersionCriteria();
            return this;
        }

        public IAliasFactory ContentType() {
            return Named("ct");
        }
    }

    public class DefaultHqlExpressionFactory : IHqlExpressionFactory {
        public IHqlCriterion Criterion { get; private set; }

        public void Eq(string propertyName, object value) {
            Criterion = HqlRestrictions.Eq(propertyName, value);
        }

        public void Like(string propertyName, string value, HqlMatchMode matchMode) {
            Criterion = HqlRestrictions.Like(propertyName, value, matchMode);
        }

        public void InsensitiveLike(string propertyName, string value, HqlMatchMode matchMode) {
            Criterion = HqlRestrictions.InsensitiveLike(propertyName, value, matchMode);
        }

        public void Gt(string propertyName, object value) {
            Criterion = HqlRestrictions.Gt(propertyName, value);
        }

        public void Lt(string propertyName, object value) {
            Criterion = HqlRestrictions.Lt(propertyName, value);
        }

        public void Le(string propertyName, object value) {
            Criterion = HqlRestrictions.Le(propertyName, value);
        }

        public void Ge(string propertyName, object value) {
            Criterion = HqlRestrictions.Ge(propertyName, value);
        }

        public void Between(string propertyName, object lo, object hi) {
            Criterion = HqlRestrictions.Between(propertyName, lo, hi);
        }

        public void In(string propertyName, object[] values) {
            Criterion = HqlRestrictions.In(propertyName, values);
        }

        public void In(string propertyName, ICollection values) {
            Criterion = HqlRestrictions.In(propertyName, values);
        }

        public void InG<T>(string propertyName, ICollection<T> values) {
            Criterion = HqlRestrictions.InG(propertyName, values);
        }

        public void IsNull(string propertyName) {
            Criterion = HqlRestrictions.IsNull(propertyName);
        }

        public void EqProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.EqProperty(propertyName, otherPropertyName);
        }

        public void NotEqProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.NotEqProperty(propertyName, otherPropertyName);
        }

        public void GtProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.GtProperty(propertyName, otherPropertyName);
        }

        public void GeProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.GeProperty(propertyName, otherPropertyName);
        }

        public void LtProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.LtProperty(propertyName, otherPropertyName);
        }

        public void LeProperty(string propertyName, string otherPropertyName) {
            Criterion = HqlRestrictions.LeProperty(propertyName, otherPropertyName);
        }

        public void IsNotNull(string propertyName) {
            Criterion = HqlRestrictions.IsNotNull(propertyName);
        }

        public void IsNotEmpty(string propertyName) {
            Criterion = HqlRestrictions.IsNotEmpty(propertyName);
        }

        public void IsEmpty(string propertyName) {
            Criterion = HqlRestrictions.IsEmpty(propertyName);
        }

        public void And(Action<IHqlExpressionFactory> lhs, Action<IHqlExpressionFactory> rhs) {
            lhs(this);
            var a = Criterion;
            rhs(this);
            var b = Criterion;
            Criterion = HqlRestrictions.And(a, b);
        }

        public void Or(Action<IHqlExpressionFactory> lhs, Action<IHqlExpressionFactory> rhs) {
            lhs(this);
            var a = Criterion;
            rhs(this);
            var b = Criterion;
            Criterion = HqlRestrictions.Or(a, b);
        }

        public void Not(Action<IHqlExpressionFactory> expression) {
            expression(this);
            var a = Criterion;
            Criterion = HqlRestrictions.Not(a);
        }

        public void Conjunction(Action<IHqlExpressionFactory> expression, params Action<IHqlExpressionFactory>[] otherExpressions) {
            var junction = HqlRestrictions.Conjunction();
            foreach (var exp in Enumerable.Empty<Action<IHqlExpressionFactory>>().Union(new[] { expression }).Union(otherExpressions)) {
                exp(this);
                junction.Add(Criterion);
            }

            Criterion = junction;
        }

        public void Disjunction(Action<IHqlExpressionFactory> expression, params Action<IHqlExpressionFactory>[] otherExpressions) {
            var junction = HqlRestrictions.Disjunction();
            foreach (var exp in Enumerable.Empty<Action<IHqlExpressionFactory>>().Union(new[] { expression }).Union(otherExpressions)) {
                exp(this);
                junction.Add(Criterion);
            }

            Criterion = junction;
        }

        public void AllEq(IDictionary propertyNameValues) {
            Criterion = HqlRestrictions.AllEq(propertyNameValues);
        }

        public void NaturalId() {
            Criterion = HqlRestrictions.NaturalId();
        }
    }

    public enum HqlMatchMode {
        Exact,
        Start,
        End,
        Anywhere
    }

}
