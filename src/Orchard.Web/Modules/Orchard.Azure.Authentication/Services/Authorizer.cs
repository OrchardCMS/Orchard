using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Services;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.Azure.Authentication.CRUD;
using Orchard.Azure.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
//using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web.Security;

namespace Orchard.Azure.Authentication.Services
{
	[OrchardSuppressDependency("Orchard.Security.Authorizer")]
	public class Authorizer : IAzureAuthorizer 
	{
		private static int _minutesToCacheADGroupMembership = -1;
		private static bool _useAzureGraphApi = false;
		private const int Hashed = 1;

		private readonly IAuthorizationService _authorizationService;
		private readonly INotifier _notifier;
		private readonly IContentManager _contentManager;
		private readonly IClock _clock;
		private readonly IRoleService _roleService;
		private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
		private readonly IRepository<AzureActiveDirectorySyncPartRecord> _azureActiveDirectorySyncPartRepository;
		private readonly IAzureGraphApiPersistence _azureGraphApiPersistence;
		private readonly IHttpContextPersistence _httpContextPersistence;
		private readonly IOrchardUserRolesPersistence _orchardUserRolesPersistence;
        private readonly ISiteService _siteService;

		public Authorizer(IAuthorizationService authorizationService, INotifier notifier, IContentManager contentManager, 
            IRoleService roleService, IRepository<UserRolesPartRecord> userRolesRepository, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository, 
            IClock clock, IHttpContextPersistence httpContextPersistence, IOrchardUserRolesPersistence orchardUserRolesPersistence, 
            IAzureGraphApiPersistence azureGraphApiPersistence, ISiteService siteService)
		{
			_authorizationService = authorizationService;
			_azureActiveDirectorySyncPartRepository = azureActiveDirectorySyncPartRepository;
			_clock = clock;
			_notifier = notifier;
			_contentManager = contentManager;
			_roleService = roleService;
			_userRolesRepository = userRolesRepository;
			_httpContextPersistence = httpContextPersistence;
			_orchardUserRolesPersistence = orchardUserRolesPersistence;
			_azureGraphApiPersistence = azureGraphApiPersistence;
            _siteService = siteService;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		private int MinutesToCacheADGroupMembership
		{
			get
			{
				if (_minutesToCacheADGroupMembership < 0)
				{
					var settings = _siteService.GetSiteSettings().As<AzureSettingsPart>();
					_minutesToCacheADGroupMembership = settings.MinutesToCacheADGroupMembership;
					_useAzureGraphApi = settings.UseAzureGraphApi;
				}

				return _minutesToCacheADGroupMembership;
			}
			set { _minutesToCacheADGroupMembership = value; }
		}

		private bool UseAzureGraphApi
		{
			get
			{
				if (_minutesToCacheADGroupMembership < 0)
				{
                    var settings = _siteService.GetSiteSettings().As<AzureSettingsPart>();
                    _minutesToCacheADGroupMembership = settings.MinutesToCacheADGroupMembership;
					_useAzureGraphApi = settings.UseAzureGraphApi;
				}

				return _useAzureGraphApi;
			}
			set { _useAzureGraphApi = value; }
		}


		public bool Authorize(Permission permission)
		{
			return Authorize(permission, null, null);
		}

		public bool Authorize(Permission permission, LocalizedString message)
		{
			return Authorize(permission, null, message);
		}

		public bool Authorize(Permission permission, IContent content)
		{
			return Authorize(permission, content, null);
		}

		public bool Authorize(Permission permission, IContent content, LocalizedString message)
		{
			bool output = false;
			string userName = _httpContextPersistence.GetAuthenticatedUserName();
			IUser user = GetOrCreateOrchardUser(userName, _azureActiveDirectorySyncPartRepository, _clock);

			// attempts to authorize the active directory user based on their roles
			// and the permissions that their associated roles have.
			if (_authorizationService.TryCheckAccess(permission, user, content))
				output = true;
			else
			{
				if (null != message)
					_notifier.Add(NotifyType.Error, T("{0}. Current user, {2}, does not have {1} permission.", message, permission.Name, user.UserName));
			}

			return output;
		}

		/// <summary>
		/// Loop through the Azure active directory groups, if the group name matches an
		/// orchard role then the user is assigned to that Orchard role.
		/// </summary>
		/// <param name="user">Orchard User who will have the Orchard roles updated.</param>
		/// <param name="activeDirectoryRoles">Azure Graph API roles</param>
		private void AddUserToOrchardRoles(IUser user, IList<string> activeDirectoryRoles)
		{
			var availableRoles = _roleService.GetRoles();

			foreach (var activeDirectoryRole in activeDirectoryRoles)
			{
				var orchardRole = availableRoles.Where(x => x.Name.ToLower() == activeDirectoryRole.ToLower()).SingleOrDefault();

				if (orchardRole != null)
					_userRolesRepository.Create(new UserRolesPartRecord { Role = orchardRole, UserId = user.Id });
			}
		}

		/// <summary>
		/// Query the Orchard DB to see if an orchard username matches the authenticated Azure active directory userName.
		/// Create a new Orchard User if required.
		/// </summary>
		/// <param name="activeDirectoryUser">Currently logged in active directory user.</param>
		/// <returns>Returns the user that was created, or if one wasn't created then the
		/// UserPart that is already in the database is returned.</returns>
		public IUser GetOrCreateOrchardUser(string userName, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository, IClock clock)
		{
			IUser user = GetUser(userName);

			if (user == null && !String.IsNullOrEmpty(userName))
			{
				string password = Guid.NewGuid().ToString("N"); // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx (32 alphanumeric characters, no hyphen separators)
				//Note this password is never going to be used.  Eachuser will be authenticated with AzureAD, not an Orchard Password
				user = CreateOrchardUser(new CreateUserParams(userName, password, userName, String.Empty, String.Empty, true));
				if (UseAzureGraphApi)
				{
					List<string> roles = _azureGraphApiPersistence.ReadUserRolesFromAzureActiveDirectoryGraphApi(userName);
					AddUserToOrchardRoles(user, roles);
				}
			}
			else if(null != user && UseAzureGraphApi)
			{
				UpdateUserRolesWithActiveDirectoryGroupMembership(user, azureActiveDirectorySyncPartRepository, clock);
			}

			return user;
		}

		/// <summary>
		/// CreateOrchardUser
		/// </summary>
		/// <param name="createUserParams"></param>
		private IUser CreateOrchardUser(CreateUserParams createUserParams)
		{
			var user = _contentManager.New<UserPart>("User");

			user.Record.UserName = createUserParams.Username;
			user.Record.Email = createUserParams.Email;
			user.Record.NormalizedUserName = createUserParams.Username.ToLowerInvariant();
			user.Record.HashAlgorithm = "SHA1";
			user.Record.RegistrationStatus = UserStatus.Approved;
			user.Record.EmailStatus = UserStatus.Approved;
			SetPasswordHashed(user.Record, createUserParams.Password);

			_contentManager.Create(user);

			return user;
		}

		/// <summary>
		/// GetEmail
		/// </summary>
		/// <param name="userName"></param>
		/// <returns>string</returns>
		private string GetEmail(string userName)
		{
			return userName;
		}

		/// <summary>
		/// GetUser
		/// </summary>
		/// <param name="username">Username of the active directory user.</param>
		/// <returns>IUser</returns>
		private IUser GetUser(string username)
		{
			string lowerName = username == null ? "" : username.ToLowerInvariant();
			return _orchardUserRolesPersistence.GetUser(lowerName, _contentManager);
		}

		/// <summary>
		/// Sets a fake password on the Orchard User. This password will never be used as the users
		/// will automatically be logged in via Azure active directory.
		/// </summary>
		/// <param name="partRecord"></param>
		/// <param name="password"></param>
		private static void SetPasswordHashed(UserPartRecord partRecord, string password)
		{
			var saltBytes = new byte[0x10];
			using (var random = new RNGCryptoServiceProvider())
			{
				random.GetBytes(saltBytes);
			}

			var passwordBytes = Encoding.Unicode.GetBytes(password);

			var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

			byte[] hashBytes;
			using (var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm))
			{
				hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
			}

			partRecord.PasswordFormat = MembershipPasswordFormat.Hashed;
			partRecord.Password = Convert.ToBase64String(hashBytes);
			partRecord.PasswordSalt = Convert.ToBase64String(saltBytes);
		}

		private void UpdateUserRolesWithActiveDirectoryGroupMembership(IUser user, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository, IClock clock)
		{
			if (NeedToReSynchronizeWithActiveDirectory(user, azureActiveDirectorySyncPartRepository, clock))
			{
				IList<string> activeDirectoryRoles = _azureGraphApiPersistence.ReadUserRolesFromAzureActiveDirectoryGraphApi(user.UserName);
				List<string> availableOrchardRoles = _orchardUserRolesPersistence.GetRoleNames(_roleService);
				List<string> currentOrchardRoles = _orchardUserRolesPersistence.GetRoleNamesForUserId(_userRolesRepository, user.Id);
				List<string> userRoleRecordsToCreate = new List<string>();
				List<string> userRoleRecordsToDelete = new List<string>();

				// loops through the active directory roles trying to match to an
				// orchard role, if one is found then the user is assigned to that
				// role.
				foreach (string activeDirectoryRole in activeDirectoryRoles)
				{
					string availableRole = availableOrchardRoles.Where(x => x.ToLowerInvariant() == activeDirectoryRole.ToLowerInvariant() && IsActiveDirectoryRoleName(x)).SingleOrDefault();
					if (availableRole != null)
					{
						string roleToAdd = currentOrchardRoles.Where(x => x.ToLowerInvariant() == activeDirectoryRole.ToLowerInvariant() && IsActiveDirectoryRoleName(x)).SingleOrDefault();
						if (null == roleToAdd)
							userRoleRecordsToCreate.Add(availableRole);
					}
				}
				//It is possible that the user has been removed from the AD Group and needs to be removed form the Orchard Role
				foreach (string roleName in currentOrchardRoles)
				{
					if (IsActiveDirectoryRoleName(roleName))
					{
						string orchardRole = activeDirectoryRoles.Where(x => x.ToLower() == roleName.ToLower()).SingleOrDefault();

						if (null == orchardRole)
							userRoleRecordsToDelete.Add(roleName);
					}
				}

				_orchardUserRolesPersistence.UpdateUserRoles(user.Id, userRoleRecordsToCreate, userRoleRecordsToDelete, _userRolesRepository, _roleService);

				PersistLastSyncedUtcToDB(user, azureActiveDirectorySyncPartRepository, clock);
			}
		}

		private bool IsActiveDirectoryRoleName(string roleName)
		{
			return 
				(
					false == string.IsNullOrWhiteSpace(roleName) 
					&& (roleName.Contains("\\") || roleName.Contains("/"))
				);
		}

		/// <summary>
		/// Read from a new table (named "RadioSystems_AzureAuthentication_AzureActiveDirectorySyncPartRecord") the last time we synced this User with AD, 
		/// if it's been over an x minutes (read from config setting) since we synced with AD
		/// </summary>
		/// <param name="user"></param>
		/// <param name="azureActiveDirectorySyncPartRepository"></param>
		/// <returns></returns>
		private bool NeedToReSynchronizeWithActiveDirectory(IUser user, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository, IClock clock)
		{
			bool output = true;

			if (UseAzureGraphApi && IsActiveDirectoryUserName(user) )
			{
				AzureActiveDirectorySyncPartRecord azureActiveDirectorySyncPartRecord = azureActiveDirectorySyncPartRepository.Fetch(x => x.UserId == user.Id).FirstOrDefault();
				int minutes = MinutesToCacheADGroupMembership;
				output = (
					null == azureActiveDirectorySyncPartRecord
					|| null == azureActiveDirectorySyncPartRecord.LastSyncedUtc
					|| azureActiveDirectorySyncPartRecord.LastSyncedUtc < clock.UtcNow.AddMinutes(-1 * minutes)
					);

				if (null == azureActiveDirectorySyncPartRecord)
				{
					//Let's create a row in the "RadioSystems_AzureAuthentication_AzureActiveDirectorySyncPartRecord" table for the current user
					azureActiveDirectorySyncPartRecord = new AzureActiveDirectorySyncPartRecord();
					azureActiveDirectorySyncPartRecord.UserId = user.Id;
					azureActiveDirectorySyncPartRecord.LastSyncedUtc = SqlServerMinDateTime();
					azureActiveDirectorySyncPartRecord.ContentItemRecord = new ContentItemRecord();

					azureActiveDirectorySyncPartRepository.Create(azureActiveDirectorySyncPartRecord);
					azureActiveDirectorySyncPartRepository.Flush();
				}
			}
			else
				output = false;

			return output;
		}

		private bool IsActiveDirectoryUserName(IUser user)
		{
			bool output = false;
			if(null != user)
				output = IsActiveDirectoryUserName(user.UserName);

			return output;
		}

		private bool IsActiveDirectoryUserName(string userName)
		{
			return ( false == string.IsNullOrWhiteSpace(userName) && (userName.Contains("@")) );
		}


		private static DateTime SqlServerMinDateTime()
		{
			return new DateTime(1753, 01, 01, 20, 0, 0, DateTimeKind.Utc);
		}

		private void PersistLastSyncedUtcToDB(IUser user, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository, IClock clock)
		{
			AzureActiveDirectorySyncPartRecord azureActiveDirectorySyncPartRecord = azureActiveDirectorySyncPartRepository.Fetch(x => x.UserId == user.Id).FirstOrDefault();
			if (null == azureActiveDirectorySyncPartRecord)
			{
				// We should never fall into this if block, but in case we do, let's create a row in the 
				// "RadioSystems_AzureAuthentication_AzureActiveDirectorySyncPartRecord" table for the current user
				azureActiveDirectorySyncPartRecord = new AzureActiveDirectorySyncPartRecord();
				azureActiveDirectorySyncPartRecord.UserId = user.Id;
				azureActiveDirectorySyncPartRecord.LastSyncedUtc = clock.UtcNow;
				azureActiveDirectorySyncPartRecord.ContentItemRecord = new ContentItemRecord();

				azureActiveDirectorySyncPartRepository.Create(azureActiveDirectorySyncPartRecord);
			}
			else
			{
				azureActiveDirectorySyncPartRecord.UserId = user.Id;
				azureActiveDirectorySyncPartRecord.LastSyncedUtc = clock.UtcNow;

				azureActiveDirectorySyncPartRepository.Update(azureActiveDirectorySyncPartRecord);
			}
		}

	}
}
