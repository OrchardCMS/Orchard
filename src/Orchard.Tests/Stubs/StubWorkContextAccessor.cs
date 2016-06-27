using System;
using System.Collections.Generic;
using System.Web;
using Autofac;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.Tests.Stubs {
    public class StubWorkContextAccessor : IWorkContextAccessor {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly WorkContext _workContext;

        public StubWorkContextAccessor(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
            _workContext = new WorkContextImpl(lifetimeScope);
        }

        public class WorkContextImpl : WorkContext {
            private readonly ILifetimeScope _lifetimeScope;
            private readonly Dictionary<string, object> _contextDictonary;

            public delegate void MyInitMethod(WorkContextImpl workContextImpl);

            public static MyInitMethod _initMethod;

            public WorkContextImpl(ILifetimeScope lifetimeScope) {
                _contextDictonary = new Dictionary<string, object>();
                CurrentUser = new StubUser();
                var ci = new ContentItem();
                ci.Weld(new StubSite());
                CurrentSite = ci.As<ISite>();
                _lifetimeScope = lifetimeScope;

                if (_initMethod != null) {
                    _initMethod(this);
                }
            }

            public class StubSite : ContentPart, ISite {
                public static string DefaultSuperUser;

                public string PageTitleSeparator {
                    get { throw new NotImplementedException(); }
                }

                public string SiteName {
                    get { throw new NotImplementedException(); }
                }

                public string SiteSalt {
                    get { throw new NotImplementedException(); }
                }

                public string SuperUser {
                    get { return DefaultSuperUser; }
                }

                public string HomePage {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                public string SiteCulture {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

				public string SiteCalendar {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                public ResourceDebugMode ResourceDebugMode {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                 public bool UseCdn {
                     get { throw new NotImplementedException(); }
                     set { throw new NotImplementedException(); }
                 }

                public int PageSize {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                public int MaxPageSize {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }
                
                public int MaxPagedCount {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                public string BaseUrl { get; set;}

                public string SiteTimeZone { get; set; }
            }

            public class StubUser : IUser {
                public ContentItem ContentItem {
                    get { throw new NotImplementedException(); }
                }

                public int Id {
                    get { return 5; }
                }

                public string UserName {
                    get { return "Fake"; }
                }

                public string Email {
                    get { return "Fake@fake.com"; }
                }
            }

            public override T Resolve<T>() {
                return _lifetimeScope.Resolve<T>();
            }

            public override object Resolve(Type serviceType) {
                return _lifetimeScope.Resolve(serviceType);
            }

            public override bool TryResolve<T>(out T service) {
                return _lifetimeScope.TryResolve<T>(out service);
            }

            public override bool TryResolve(Type serviceType, out object service) {
                return _lifetimeScope.TryResolve(serviceType, out service);
            }

            public override T GetState<T>(string name) {
                return (T) _contextDictonary[name];
            }

            public override void SetState<T>(string name, T value) {
                _contextDictonary[name] = value;
            }
        }

        public WorkContext GetContext(HttpContextBase httpContext) {
            return _workContext;
        }

        public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
            throw new NotSupportedException();
        }

        public WorkContext GetContext() {
            return _workContext;
        }

        public IWorkContextScope CreateWorkContextScope() {
            var workLifetime = _lifetimeScope.BeginLifetimeScope("work");
            var workContext = new WorkContextImpl(workLifetime);
            return new StubWorkContextScope(workContext, workLifetime);
        }
    }
}
