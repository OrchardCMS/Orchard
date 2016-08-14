using System.IO;
using Orchard;
using System.Collections.Generic;

namespace Orchard.Azure.Authentication.CRUD
{
	public interface IAzureGraphApiPersistence : IDependency
	{
		List<string> ReadUserRolesFromAzureActiveDirectoryGraphApi(string username);
		Stream ReadUserThumbnailPhotoFromGraphApi(string userName);
	}
}
