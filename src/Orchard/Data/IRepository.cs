using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Orchard.ContentManagement.Records;

namespace Orchard.Data {
    public interface IRepository<T> {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Copy(T source, T target);

        T Get(int id);
        T Get(Expression<Func<T, bool>> predicate);

        IQueryable<T> Table { get; }

        int Count(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip, int count);

        [Obsolete]
        void Transaction(Action action);
    }
}