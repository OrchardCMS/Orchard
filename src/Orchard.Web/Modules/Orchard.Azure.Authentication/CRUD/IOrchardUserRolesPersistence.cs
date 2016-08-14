using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Roles.Services;
using Orchard.Roles.Models;
using Orchard.Security;
using System;
using System.Collections.Generic;

namespace Orchard.Azure.Authentication.CRUD
{
	public interface IOrchardUserRolesPersistence : IDependency
	{
		List<string> GetRoleNames(IRoleService roleService);
		List<string> GetRoleNamesForUserId(IRepository<UserRolesPartRecord> userRolesRepository, int userId);
		IUser GetUser(string username, IContentManager contentManager);
		void UpdateUserRoles(int userId, List<string> userRoleNamesToCreate, List<string> userRoleNamesToDelete, IRepository<UserRolesPartRecord> userRolesRepository, IRoleService roleService);
	}
}
