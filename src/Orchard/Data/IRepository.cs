using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Orchard.Data {
    public interface IRepository<T> {
        void Create(T entity);
        void Delete(T entity);
        void Copy(T source, T target);
        void Flush();

        T Get(int id);
        T Get(Expression<Func<T, bool>> predicate);

        int Count(Expression<Func<T, bool>> predicate);

        IEnumerable<T> Fetch();
        IEnumerable<T> Fetch(Action<Orderable<T>> order);
        IEnumerable<T> Fetch(Action<Orderable<T>> order, int skip, int count);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order);
        IEnumerable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip, int count);
    }
}