using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Orchard.Specs.Util;

namespace Orchard.Specs.Hosting
{
    public static class RequestExtensions
    {
        public static RequestDetails SendRequest(this WebHost webHost, string urlPath)
        {
            var physicalPath = Bleroy.FluentPath.Path.Get(webHost.PhysicalDirectory);

            var details = new RequestDetails
            {
                Page = physicalPath
                    .Combine(urlPath.TrimStart('/', '\\'))
                    .GetRelativePath(physicalPath)
            };

            webHost.Execute(() =>
            {
                var output = new StringWriter();
                HttpRuntime.ProcessRequest(new Worker(details, output));
                details.ResponseText = output.ToString();
            });

            return details;
        }

        class Worker : SimpleWorkerRequest
        {
            private readonly RequestDetails _details;
            private readonly TextWriter _output;

            public Worker(RequestDetails details, TextWriter output)
                : base(details.Page, details.Query, output)
            {
                _details = details;
                _output = output;
            }

            public override void SendStatus(int statusCode, string statusDescription)
            {
                _details.StatusCode = statusCode;
                _details.StatusDescription = statusDescription;

                base.SendStatus(statusCode, statusDescription);
            }

            public override void SendResponseFromFile(string filename, long offset, long length)
            {
                _output.Write(File.ReadAllText(filename));
            }
        }
    }
}

