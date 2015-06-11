using System.Collections.Specialized;
using System.Web;
using Orchard.DynamicForms.Models;

namespace Orchard.DynamicForms.Helpers {
    public static class SubmissionExtensions {
        public static NameValueCollection ToNameValues(this Submission submission) {
            return HttpUtility.ParseQueryString(submission.FormData);
        }
    }
}