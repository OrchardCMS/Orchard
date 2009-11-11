using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Roles.Models;

namespace Orchard.Roles.Services {
    public interface IRoleService : IDependency {
        IEnumerable<RoleRecord> GetRoles();
        RoleRecord GetRole(int id);
        void CreateRole(string roleName);
        void UpdateRole(int id, string roleName);
        void DeleteRole(int id);
    }

    public class RoleService : IRoleService {
        private readonly IRepository<RoleRecord> _roleRepository;

        public RoleService(IRepository<RoleRecord> roleRepository) {
            _roleRepository = roleRepository;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of IRoleService

        public IEnumerable<RoleRecord> GetRoles() {
            var roles = from role in _roleRepository.Table select role;
            return roles.ToList();
        }

        public RoleRecord GetRole(int id) {
            return _roleRepository.Get(id);
        }

        public void CreateRole(string roleName) {
            _roleRepository.Create(new RoleRecord { Name = roleName });
        }

        public void UpdateRole(int id, string roleName) {
            _roleRepository.Update(new RoleRecord { Id = id, Name = roleName });
        }

        public void DeleteRole(int id) {
            _roleRepository.Delete(new RoleRecord { Id = id });
        }

        #endregion
    }
}
