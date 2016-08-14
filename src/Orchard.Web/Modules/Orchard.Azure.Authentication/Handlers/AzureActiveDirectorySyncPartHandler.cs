using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Azure.Authentication.Models;

namespace Orchard.Azure.Authentication.Handler
{
	public class AzureActiveDirectorySyncPartHandler : ContentHandler
	{
		public AzureActiveDirectorySyncPartHandler(IRepository<AzureActiveDirectorySyncPartRecord> repository)
		{
			Filters.Add(StorageFilter.For(repository));
		}
	}
}