using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace IDeliverable.Licensing.Service
{
    public class ApiKeyAuthorizationAttribute : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			IEnumerable<string> values = null;
			actionContext.Request.Headers.TryGetValues("ApiKey", out values);
			string apiKey = values != null ? values.FirstOrDefault() : null;

			if (String.IsNullOrWhiteSpace(apiKey))
				actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized);

            var expectedApiKey = ConfigurationManager.AppSettings["ApiKey"];

			if (apiKey != expectedApiKey)
				actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
		}
	}
}