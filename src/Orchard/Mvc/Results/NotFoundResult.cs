using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Mvc.Results {
    public class NotFoundResult : ViewResult {
        public override void ExecuteResult(ControllerContext context) {
            throw new HttpException((int)HttpStatusCode.NotFound, "Resource not found");
        }
    }
}