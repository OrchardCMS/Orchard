using System;
using System.Web;
using Orchard.Services;

namespace Orchard.Comments.Services {
    public class HtmlEncodeFilter : IHtmlFilter {
        public string ProcessContent(string text) {

            return HttpUtility.HtmlEncode(Convert.ToString(text)).Replace("\r\n", "<br />\r\n");
        }
    }
}