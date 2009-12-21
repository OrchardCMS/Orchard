using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Sandbox.Models {
    public class SandboxPage : ContentPart<SandboxPageRecord> {

        public readonly static ContentType ContentType = new ContentType {Name = "sandboxpage", DisplayName = "Sandbox Page"};

    }
}
