using Orchard.Mvc;
using System.Security.Principal;

namespace Orchard.Azure.Authentication.CRUD
{
	public class HttpContextPersistence : IHttpContextPersistence
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public HttpContextPersistence(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public string GetAuthenticatedUserName()
		{
			string output = null;
			IPrincipal azureUser = _httpContextAccessor.Current().User;

			if (azureUser.Identity.IsAuthenticated)
				output = azureUser.Identity.Name.Trim();
	
			return output; 
		}
	}
}