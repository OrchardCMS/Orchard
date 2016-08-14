using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Roles;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Users;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Azure.Authentication.CRUD
{
	public class OrchardUserRolesPersistence : IOrchardUserRolesPersistence
	{

		/// <summary>
		/// GetRoleNames
		/// </summary>
		/// <param name="roleService">roleService</param>
		/// <returns>List<string></returns>
		public List<string> GetRoleNames(IRoleService roleService)
		{
			List<string> output = new List<string>();
			IEnumerable<RoleRecord> roleRecords = roleService.GetRoles();
			foreach (RoleRecord roleRecord in roleRecords)
			{
				output.Add(roleRecord.Name);
			}

			return output;
		}

		/// <summary>
		/// GetRoleNamesForUserId
		/// </summary>
		/// <param name="userRolesRepository">userRolesRepository</param>
		/// <param name="userId">userId</param>
		/// <returns>List<string></returns>
		public List<string> GetRoleNamesForUserId(IRepository<UserRolesPartRecord> userRolesRepository, int userId)
		{
			List<string> output = new List<string>();
			IEnumerable<UserRolesPartRecord> userRolesPartRecords = userRolesRepository.Fetch(x => x.UserId == userId);
			foreach (UserRolesPartRecord userRolesPartRecord in userRolesPartRecords)
			{
				output.Add(userRolesPartRecord.Role.Name);
			}

			return output;
		}

		/// <summary>
		/// GetUser
		/// IContentManager.Query is a static extension method and therefore not directly mockable 
		/// Also, UserPart is not serializable due to circular object references, and not deserializable as well
		/// Solution: hide the non-mockable and non-serializable junk in this class, then mock out IOrchardUserRolesPersistence.
		/// </summary>
		/// <param name="username">username</param>
		/// <param name="contentManager">contentManager</param>
		/// <returns>IUser</returns>
		public IUser GetUser(string username, IContentManager contentManager)
		{
			var lowerName = username == null ? "" : username.ToLowerInvariant();
			return contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();
		}

		/// <summary>
		/// UpdateUserRoles
		/// </summary>
		/// <param name="userId">userId</param>
		/// <param name="userRoleNamesToCreate">userRoleNamesToCreate</param>
		/// <param name="userRoleNamesToDelete">userRoleNamesToDelete</param>
		/// <param name="userRolesRepository">userRolesRepository</param>
		/// <param name="roleService">roleService</param>
		public void UpdateUserRoles(int userId, List<string> userRoleNamesToCreate, List<string> userRoleNamesToDelete, IRepository<UserRolesPartRecord> userRolesRepository, IRoleService roleService)
		{
			foreach (string roleName in userRoleNamesToCreate)
			{
				RoleRecord roleRecord = roleService.GetRoleByName(roleName);
				if (null != roleRecord)
					userRolesRepository.Create(new UserRolesPartRecord { Role = roleRecord, UserId = userId });
			}
			foreach (string roleName in userRoleNamesToDelete)
			{
				RoleRecord roleRecord = roleService.GetRoleByName(roleName);
				if (null != roleRecord)
					userRolesRepository.Delete(new UserRolesPartRecord { Role = roleRecord, UserId = userId });
			}
		}

	}
}