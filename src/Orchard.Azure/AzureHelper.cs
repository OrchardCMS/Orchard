using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace Orchard.Azure {
    public class AzureHelper {
        public static void InjectHttpContext(Action action) {
            if(HttpContext.Current == null )
            {
                action();
                return;
            }

            var currentContext = HttpContext.Current;
            try {
                // THIS IS A HACK
                // There is a bug in ASP.NET 4.0 in HttpEncoder.Current, which prevents some calls to HttpUtiliy.Decode/Encode
                // from Application_Start, on IIS or Azure. This hack will be removed when the bug is corrected.
                // This is fired by the assembly Microsoft.WindowsAzure.StorageClient. Should be corrected in .NET4 SP1

                HttpContext.Current = new HttpContext(new HttpRequest(String.Empty, "http://localhost", String.Empty), new HttpResponse(new StringWriter()));

                action();
            }
            finally {
                HttpContext.Current = currentContext;
            }
        }
    }
}
