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
    internal class ApiKeyAuthorizationAttribute : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
            var values = new List<string>();

            var paramsQuery =
                from param in actionContext.Request.GetQueryNameValuePairs()
                where String.Equals(param.Key, "ApiKey", StringComparison.OrdinalIgnoreCase)
                select param.Value;
            values.AddRange(paramsQuery.ToArray());

            IEnumerable<string> headers;
			actionContext.Request.Headers.TryGetValues("ApiKey", out headers);
            if (headers != null)
                values.AddRange(headers);

			string apiKey = values != null ? values.FirstOrDefault() : null;

			if (String.IsNullOrWhiteSpace(apiKey))
				actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized);

            var expectedApiKey = ConfigurationManager.AppSettings["ApiKey"];

			if (apiKey != expectedApiKey)
				actionContext.Response = actionContext.ControllerContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
		}
	}
}