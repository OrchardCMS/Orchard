using Orchard.Data;
using Orchard.Security;
using Orchard.Services;
using Orchard.Azure.Authentication.Models;

namespace Orchard.Azure.Authentication.Services
{
	public interface IAzureAuthorizer : IAuthorizer
	{
		IUser GetOrCreateOrchardUser(string userName, IRepository<AzureActiveDirectorySyncPartRecord> azureActiveDirectorySyncPartRepository, IClock clock);
	}
}