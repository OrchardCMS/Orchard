using System.IO;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Settings;
using Orchard.Azure.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using LogLevel = Orchard.Logging.LogLevel;

namespace Orchard.Azure.Authentication.CRUD
{
	public class AzureGraphApiPersistence : IAzureGraphApiPersistence
	{
        public ILogger Logger { get; set; }

		private static List<string> _adGroupNamesInOrchard = null;
		private readonly IRoleService _roleService;

		private static string _appKey = ConfigurationManager.AppSettings["ActiveDirectoryAuthorization_AppKey"];
		private static string _azureADTenantUrl;
		private static string _azureTenantName;
		private static string _clientId;
		private static string _graphApiUrl;

		public AzureGraphApiPersistence(ISiteService siteService, IRoleService roleService)
		{
            Logger = NullLogger.Instance;

			_roleService = roleService;

            try
            {
                //Read Settings From Orchard
                var site = siteService.GetSiteSettings();
                var settings = site.As<AzureSettingsPart>();
                //settings should never be null, and there should always be 1 row in the table
                _azureADTenantUrl = settings.ADInstance;
				_azureTenantName = settings.Tenant;
				_clientId = settings.ClientId;
				_graphApiUrl = settings.GraphApiUrl;
			}
			catch (Exception ex)
			{
				Logger.Log(LogLevel.Debug, ex, "An error occured while accessing azure settings: {0}");
			}
		}

		public List<string> ReadUserRolesFromAzureActiveDirectoryGraphApi(string username)
		{
			if (null == _adGroupNamesInOrchard)
			{
				_adGroupNamesInOrchard = ReadADGroupNamesFromOrchardDB(_roleService);
			}

			List<string> groupGuidsForUser = ReadUserGroupGuidsFromGraphApi(username);
			//groupGuidsForUser = CheckMemberGroupsFromGraphApi(username, _adGroupNamesInOrchard);
			Dictionary<string, string> adGroupGuidNameMapping = ReadGroupObjectsFromAzureActiveDirectoryGraphApi(groupGuidsForUser);
			List<string> output = ConvertGroupGuidsToGroupNames(_adGroupNamesInOrchard, adGroupGuidNameMapping);
			return output;
		}

		private List<string> ReadADGroupNamesFromOrchardDB(IRoleService roleService)
		{
			List<string> output = new List<string>();
			IEnumerable<RoleRecord> roleRecords = roleService.GetRoles();
			foreach (RoleRecord roleRecord in roleRecords)
			{
				if (IsActiveDirectoryRoleName(roleRecord.Name))
					output.Add(roleRecord.Name);
			}

			return output;
		}

		private bool IsActiveDirectoryRoleName(string roleName)
		{
			return
				(
					false == string.IsNullOrWhiteSpace(roleName)
					&& (roleName.Contains("\\") || roleName.Contains("/"))
				);
		}

		private List<string> ConvertGroupGuidsToGroupNames(List<string> orchardGroups, Dictionary<string, string> adGroupGuidNameMapping)
		{
			List<string> output = new List<string>();

			foreach (string orchardGroupName in orchardGroups)
			{
				if (false == string.IsNullOrWhiteSpace(orchardGroupName))
				{
					string displayName = ConvertOrchardRoleNameToDisplayName(orchardGroupName);
					if (adGroupGuidNameMapping.ContainsValue(displayName))
						output.Add(orchardGroupName);
				}	
			}

			return output;
		}

		private string ConvertOrchardRoleNameToDisplayName(string orchardGroupName)
		{
			string output = orchardGroupName;
			string[] nameParts = orchardGroupName.Split(new char[] { '\\', '/' });
			if (nameParts.Length > 1)
				output = nameParts[1].ToLowerInvariant();

			return output;
		}
		
		private Dictionary<string, string> ReadGroupObjectsFromAzureActiveDirectoryGraphApi(List<string> groupIds)
		{
			Dictionary<string, string> output = new Dictionary<string, string>();

			int chunkSize = 1000;
			int numberOfChunks = 1 + (groupIds.Count/chunkSize);
			int finalChunkSize = groupIds.Count % chunkSize;
			for (int i = 0; i < numberOfChunks; i++)
			{
				List<string> groupIdsForChunk;

				if (i == numberOfChunks-1)
				{
					// if last pass thru this loop
					groupIdsForChunk = groupIds.GetRange(i * chunkSize, finalChunkSize);
				}
				else
					groupIdsForChunk = groupIds.GetRange(i * chunkSize, chunkSize);

				ReadGroupObjectsFromAzureActiveDirectoryGraphApi2(groupIdsForChunk, ref output);
			}

			return output;
		}

		private void ReadGroupObjectsFromAzureActiveDirectoryGraphApi2(List<string> groupIds, ref Dictionary<string, string> output)
		{
			string requestUrl = string.Format(
				@"{0}/{1}/getObjectsByObjectIds?api-version=1.5",
				_graphApiUrl,
				_azureTenantName);

			string json = FormatGroupIdsAndTypes(groupIds);

			HttpResponseMessage response = AzureGraphWebServiceCall(requestUrl, HttpMethod.Post, json);
			if (response.IsSuccessStatusCode)
			{
				string responseContent = response.Content.ReadAsStringAsync().Result;
				JObject newresults = JObject.Parse(responseContent);
				JToken groups = newresults["value"];
				foreach (JObject group in groups)
				{
					try
					{
						// There is no "name" property in Azure AD, so we'll have to use mailNickname instead.  
						// If the AD group is not mail enabled, we will not be able to match the AD group to the orchard Role
						if (null != group["mailNickname"])
							output.Add(group["objectId"].ToString(), group["mailNickname"].ToString().ToLowerInvariant());
						//else
						//	output.Add(group["objectId"].ToString(), group["displayName"].ToString().ToLowerInvariant());
					}
					catch {}
				}
			}
			else
			{
				throw new System.Net.WebException("graph api call failed with error code=" + response.StatusCode);
			}


		}

		/// <summary>
		/// format json like this:
		/// {
		///		"objectIds": [
		///			"8ab3f116-1afb-44cb-8e61-6b20cb1e353c",
		///			"be78b7e2-a94a-4ab0-9bb4-403977cc7ec6",
		///			"cf61b8c9-3626-4fe4-b2f7-ac31fa905605"
		///		],
		///		"types": ["group"]
		/// }
		/// </summary>
		/// <param name="groupIds">groupIds</param>
		/// <returns>json formatted string</returns>
		private string FormatGroupIdsAndTypes(System.Collections.Generic.List<string> groupIds)
		{
			string output = "{ \"objectIds\": " + JsonConvert.SerializeObject(groupIds) + ", \"types\": [\"group\"] }";
			return output;
		}

		// GetMemberGroups might be disabled by your IT department, try calling checkMemberGroups instead
		//private List<string> GetGroupMembershipAzureActiveDirectoryGraphApi(string userName)
		private List<string> ReadUserGroupGuidsFromGraphApi(string userName)
		{
			List<string> output = new List<string>();

			string userPrincipleName = UserPrincipleName(userName);

			//GetMemberGroups searches nested groups (Transitive), use this instead of memberOf which does not handle nested groups (non-transitive)
			string requestUrl = string.Format(
				@"{0}/{1}/users/{2}/getMemberGroups?api-version=2013-04-05",
				_graphApiUrl,
				_azureTenantName,
				userPrincipleName);

			HttpResponseMessage response = AzureGraphWebServiceCall(requestUrl, HttpMethod.Post, null);
			if (response.IsSuccessStatusCode)
			{
				string responseContent = response.Content.ReadAsStringAsync().Result;
				JObject newresults = JObject.Parse(responseContent);
				JToken groups = newresults["value"];
				foreach (string group in groups)
				{
					output.Add(group);
				}
			}
			else
			{
				throw new System.Net.WebException("graph api call failed with error code=" + response.StatusCode);
			}

			return output;
		}

		private static string UserPrincipleName(string userName)
		{
			string[] nameParts = userName.Split(new char[] { '\\', '/' });
			if (nameParts.Length > 1)
			{
				string[] domainNameParts = _azureTenantName.Split(new char[] { '.' });
				if (domainNameParts.Length > 1)
				{
					userName = nameParts[1] + "@" + domainNameParts[0] + ".com";
				}
			}

			return userName;
		}

		private static HttpResponseMessage AzureGraphWebServiceCall(string requestUrl, HttpMethod httpMethod, string json)
		{
			AuthenticationContext authContext = new AuthenticationContext(_azureADTenantUrl);
			// optional: store your appkey in KeyVault: //_appKey = GetSecret(_keyVaultAddress, _keyVaultSecretName, _keyVaultClientId, _keyVaultThumbprint);

			// Acquire the Access Token to access graph API
			ClientCredential credential = new ClientCredential(_clientId, _appKey);
			
            //TODO: fix this
            //AuthenticationResult result = authContext.AcquireToken(_graphApiUrl, credential);

			HttpRequestMessage request = new HttpRequestMessage(httpMethod, requestUrl);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "bogus");
			StringContent content;
			if (HttpMethod.Get != httpMethod)
			{
				if (string.IsNullOrWhiteSpace(json))
					content = new StringContent("{\"securityEnabledOnly\": \"false\"}");
				else
					content = new StringContent(json);

				content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				request.Content = content;
			}
			HttpClient client = new HttpClient();
			HttpResponseMessage response = client.SendAsync(request).Result;
			return response;
		}

		public Stream ReadUserThumbnailPhotoFromGraphApi(string userName)
		{
			Stream output = null;

			string userPrincipleName = UserPrincipleName(userName);

			//https://graph.windows.net/myorganization/users/{user_id}/thumbnailPhoto?api-version
			string requestUrl = string.Format(
				@"{0}/{1}/users/{2}/thumbnailPhoto?api-version=2013-04-05",
				_graphApiUrl,
				_azureTenantName,
				userPrincipleName);

			HttpResponseMessage response = AzureGraphWebServiceCall(requestUrl, HttpMethod.Get, null);
			if (response.IsSuccessStatusCode)
			{
				output = response.Content.ReadAsStreamAsync().Result;
			}

			return output;
		}

	}
}