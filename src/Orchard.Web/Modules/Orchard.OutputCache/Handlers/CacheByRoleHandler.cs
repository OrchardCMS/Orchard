using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Roles.Events;
using System;
using System.Collections;
using System.Linq;

namespace Orchard.OutputCache.Handlers {
    [OrchardFeature("Orchard.OutputCache.CacheByRole")]
    public class CacheByRoleHandler : IRoleEventHandler {
        private readonly string _tenantName;
        private readonly WorkContext _workContext;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;

        // used to manage the role evict once 
        // the role evict
        private string roleEvict = string.Empty;

        public CacheByRoleHandler(
            IWorkContextAccessor workContextAccessor,
            IOutputCacheStorageProvider cacheStorageProvider,
            ShellSettings shellSettings) {
            _workContext = workContextAccessor.GetContext();
            _cacheStorageProvider = cacheStorageProvider;
            _tenantName = shellSettings.Name;
        }
        public void PermissionAdded(PermissionAddedContext context) {
            if(roleEvict != context.Role.Name) {
                roleEvict = context.Role.Name;
                EvictCache(context.Role.Name);
            }
        }

        public void PermissionRemoved(PermissionRemovedContext context) {
            if (roleEvict != context.Role.Name) {
                roleEvict = context.Role.Name;
                EvictCache(context.Role.Name);
            }
        }

        public void Removed(RoleRemovedContext context) {
            EvictCache(context.Role.Name);
        }

        public void Created(RoleCreatedContext context) {
        }

        public void Renamed(RoleRenamedContext context) {
        }

        public void UserAdded(UserAddedContext context) {
        }

        public void UserRemoved(UserRemovedContext context) {
        }

        private void EvictCache(string role) {
            // this condition works correctly, even if it has a false problem inside it, a borderline case,
            // if you create a role with the pipe inside and a similar name such as:
            // role1: aa|bb
            // role2: a|b
            // when the cache key with role a|b is evicted,
            // the cache key containing aa|bb is also evicted.
            var cacheItems = _workContext.HttpContext.Cache.AsParallel()
                 .Cast<DictionaryEntry>()
                 .Select(x => x.Value)
                 .OfType<CacheItem>()
                 // get all cache items for my tenant
                 .Where(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase))
                 // split of the cache key string
                 .Where(ci => ci.CacheKey.Split(';')
                    // each key should contain the property added for the roles
                    .Any(key => key.StartsWith("UserRoles=", StringComparison.OrdinalIgnoreCase)
                        // and the role indicated
                        && key.Substring(10).Contains(role)));

            foreach (var item in cacheItems) {
                _cacheStorageProvider.Remove(item.CacheKey);
            }
        }
    }
}