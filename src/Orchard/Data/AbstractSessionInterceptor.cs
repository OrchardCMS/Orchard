using System.Collections;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace Orchard.Data {
    /// <summary>
    /// Abstract implementation of a per-session NHibernate session interceptor.
    /// </summary>
    public abstract class AbstractSessionInterceptor : ISessionInterceptor {
        /// <summary>
        /// Called just before an object is initialized
        /// </summary>
        /// <param name="entity"/><param name="id"/><param name="propertyNames"/><param name="state"/><param name="types"/>
        /// <remarks>
        /// The interceptor may change the <c>state</c>, which will be propagated to the persistent
        ///             object. Note that when this method is called, <c>entity</c> will be an empty
        ///             uninitialized instance of the class.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the user modified the <c>state</c> in any way
        /// </returns>
        public virtual bool OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
            return false;
        }

        /// <summary>
        /// Called when an object is detected to be dirty, during a flush.
        /// </summary>
        /// <param name="currentState"/><param name="entity"/><param name="id"/><param name="previousState"/><param name="propertyNames"/><param name="types"/>
        /// <remarks>
        /// The interceptor may modify the detected <c>currentState</c>, which will be propagated to
        ///             both the database and the persistent object. Note that all flushes end in an actual
        ///             synchronization with the database, in which as the new <c>currentState</c> will be propagated
        ///             to the object, but not necessarily (immediately) to the database. It is strongly recommended
        ///             that the interceptor <b>not</b> modify the <c>previousState</c>.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the user modified the <c>currentState</c> in any way
        /// </returns>
        public virtual bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types) {
            return false;
        }

        /// <summary>
        /// Called before an object is saved
        /// </summary>
        /// <param name="entity"/><param name="id"/><param name="propertyNames"/><param name="state"/><param name="types"/>
        /// <remarks>
        /// The interceptor may modify the <c>state</c>, which will be used for the SQL <c>INSERT</c>
        ///             and propagated to the persistent object
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the user modified the <c>state</c> in any way
        /// </returns>
        public virtual bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
            return false;
        }

        /// <summary>
        /// Called before an object is deleted
        /// </summary>
        /// <param name="entity"/><param name="id"/><param name="propertyNames"/><param name="state"/><param name="types"/>
        /// <remarks>
        /// It is not recommended that the interceptor modify the <c>state</c>.
        /// </remarks>
        public virtual void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types) {
        }

        /// <summary>
        /// Called before a collection is (re)created.
        /// </summary>
        public virtual void OnCollectionRecreate(object collection, object key) {
        }

        /// <summary>
        /// Called before a collection is deleted.
        /// </summary>
        public virtual void OnCollectionRemove(object collection, object key) {
        }

        /// <summary>
        /// Called before a collection is updated.
        /// </summary>
        public virtual void OnCollectionUpdate(object collection, object key) {
        }

        /// <summary>
        /// Called before a flush
        /// </summary>
        /// <param name="entities">The entities</param>
        public virtual void PreFlush(ICollection entities) {
        }

        /// <summary>
        /// Called after a flush that actually ends in execution of the SQL statements required to
        ///             synchronize in-memory state with the database.
        /// </summary>
        /// <param name="entities">The entitites</param>
        public virtual void PostFlush(ICollection entities) {
        }

        /// <summary>
        /// Called when a transient entity is passed to <c>SaveOrUpdate</c>.
        /// </summary>
        /// <remarks>
        /// The return value determines if the object is saved
        /// <list>
        /// <item>
        /// <see langword="true"/> - the entity is passed to <c>Save()</c>, resulting in an <c>INSERT</c>
        /// </item>
        /// <item>
        /// <see langword="false"/> - the entity is passed to <c>Update()</c>, resulting in an <c>UPDATE</c>
        /// </item>
        /// <item>
        /// <see langword="null"/> - Hibernate uses the <c>unsaved-value</c> mapping to determine if the object is unsaved
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="entity">A transient entity</param>
        /// <returns>
        /// Boolean or <see langword="null"/> to choose default behaviour
        /// </returns>
        public virtual bool? IsTransient(object entity) {
            return null;
        }

        /// <summary>
        /// Called from <c>Flush()</c>. The return value determines whether the entity is updated
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>
        /// an array of property indicies - the entity is dirty
        /// </item>
        /// <item>
        /// an empty array - the entity is not dirty
        /// </item>
        /// <item>
        /// <see langword="null"/> - use Hibernate's default dirty-checking algorithm
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="entity">A persistent entity</param><param name="currentState"/><param name="id"/><param name="previousState"/><param name="propertyNames"/><param name="types"/>
        /// <returns>
        /// An array of dirty property indicies or <see langword="null"/> to choose default behavior
        /// </returns>
        public virtual int[] FindDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types) {
            return null;
        }

        /// <summary>
        /// Instantiate the entity class. Return <see langword="null"/> to indicate that Hibernate should use the default
        ///             constructor of the class
        /// </summary>
        /// <param name="entityName">the name of the entity </param><param name="entityMode">The type of entity instance to be returned. </param><param name="id">the identifier of the new instance </param>
        /// <returns>
        /// An instance of the class, or <see langword="null"/> to choose default behaviour
        /// </returns>
        /// <remarks>
        /// The identifier property of the returned instance
        ///             should be initialized with the given identifier.
        /// </remarks>
        public virtual object Instantiate(string entityName, EntityMode entityMode, object id) {
            return null;
        }

        /// <summary>
        /// Get the entity name for a persistent or transient instance
        /// </summary>
        /// <param name="entity">an entity instance </param>
        /// <returns>
        /// the name of the entity 
        /// </returns>
        public virtual string GetEntityName(object entity) {
            return null;
        }

        /// <summary>
        /// Get a fully loaded entity instance that is cached externally
        /// </summary>
        /// <param name="entityName">the name of the entity </param><param name="id">the instance identifier </param>
        /// <returns>
        /// a fully initialized entity 
        /// </returns>
        public virtual object GetEntity(string entityName, object id)
        {
            return null;
        }

        /// <summary>
        /// Called when a NHibernate transaction is begun via the NHibernate <see cref="T:NHibernate.ITransaction"/>
        ///             API. Will not be called if transactions are being controlled via some other mechanism.
        /// </summary>
        public virtual void AfterTransactionBegin(ITransaction tx) {
        }

        /// <summary>
        /// Called before a transaction is committed (but not before rollback).
        /// </summary>
        public virtual void BeforeTransactionCompletion(ITransaction tx) {
        }

        /// <summary>
        /// Called after a transaction is committed or rolled back.
        /// </summary>
        public virtual void AfterTransactionCompletion(ITransaction tx) {
        }

        /// <summary>
        /// Called when sql string is being prepared. 
        /// </summary>
        /// <param name="sql">sql to be prepared </param>
        /// <returns>
        /// original or modified sql 
        /// </returns>
        public virtual SqlString OnPrepareStatement(SqlString sql) {
            return sql;
        }

        /// <summary>
        /// Called when a session-scoped (and <b>only</b> session scoped) interceptor is attached
        ///             to a session
        /// </summary>
        /// <remarks>
        /// session-scoped-interceptor is an instance of the interceptor used only for one session.
        ///             The use of singleton-interceptor may cause problems in multi-thread scenario. 
        /// </remarks>
        /// <seealso cref="M:NHibernate.ISessionFactory.OpenSession(NHibernate.IInterceptor)"/><seealso cref="M:NHibernate.ISessionFactory.OpenSession(System.Data.IDbConnection,NHibernate.IInterceptor)"/>
        public virtual void SetSession(ISession session) {
        }
    }
}