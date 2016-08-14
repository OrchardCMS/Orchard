using Orchard;
using System;

namespace Orchard.Azure.Authentication.CRUD
{
	public interface IHttpContextPersistence : IDependency
	{
		string GetAuthenticatedUserName();
	}
}
