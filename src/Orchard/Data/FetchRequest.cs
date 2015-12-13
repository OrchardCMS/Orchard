using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;

namespace Orchard.Data {
    public interface IFetchRequest<TQueried, TFetch> : IOrderedQueryable<TQueried> { }

    public static class RepositoryFetchingExtensionMethods {
        public static IFetchRequest<TOriginating, TRelated> Fetch<TOriginating, TRelated>(this IQueryable<TOriginating> query, Expression<Func<TOriginating, TRelated>> relatedObjectSelector) {
            var fetch = EagerFetchingExtensionMethods.Fetch(query, relatedObjectSelector);
            return new FetchRequest<TOriginating, TRelated>(fetch);
        }

        public static IFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated>(this IQueryable<TOriginating> query, Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector) {
            var fecth = EagerFetchingExtensionMethods.FetchMany(query, relatedObjectSelector);
            return new FetchRequest<TOriginating, TRelated>(fecth);
        }

        public static IFetchRequest<TQueried, TRelated> ThenFetch<TQueried, TFetch, TRelated>(this IFetchRequest<TQueried, TFetch> query, Expression<Func<TFetch, TRelated>> relatedObjectSelector) {
            var impl = query as FetchRequest<TQueried, TFetch>;
            var fetch = EagerFetchingExtensionMethods.ThenFetch(impl.NhFetchRequest, relatedObjectSelector);
            return new FetchRequest<TQueried, TRelated>(fetch);
        }

        public static IFetchRequest<TQueried, TRelated> ThenFetchMany<TQueried, TFetch, TRelated>(this IFetchRequest<TQueried, TFetch> query, Expression<Func<TFetch, IEnumerable<TRelated>>> relatedObjectSelector) {
            var impl = query as FetchRequest<TQueried, TFetch>;
            var fetch = EagerFetchingExtensionMethods.ThenFetchMany(impl.NhFetchRequest, relatedObjectSelector);
            return new FetchRequest<TQueried, TRelated>(fetch);
        }
    }

    public class FetchRequest<TQueried, TFetch> : IFetchRequest<TQueried, TFetch> {
        public IEnumerator<TQueried> GetEnumerator() {
            return this.NhFetchRequest.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.NhFetchRequest.GetEnumerator();
        }

        public Type ElementType {
            get {
                return this.NhFetchRequest.ElementType;
            }
        }

        public Expression Expression {
            get {
                return this.NhFetchRequest.Expression;
            }
        }

        public IQueryProvider Provider {
            get {
                return this.NhFetchRequest.Provider;
            }
        }

        public FetchRequest(INhFetchRequest<TQueried, TFetch> nhFetchRequest) {
            this.NhFetchRequest = nhFetchRequest;
        }

        public INhFetchRequest<TQueried, TFetch> NhFetchRequest { get; private set; }
    }

}
