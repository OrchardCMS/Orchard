using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.DynamicForms.Bindings {
    [OrchardFeature("Orchard.DynamicForms.Bindings.Users")]
    public class UserPartBindings : Component, IBindingProvider {
        private readonly IMembershipService _membershipService;
        public UserPartBindings(IMembershipService membershipService) {
            _membershipService = membershipService;
        }

        public void Describe(BindingDescribeContext context) {
            context.For<UserPart>()
                .Binding("UserName", (contentItem, part, s) => {
                    part.UserName = s;
                    part.NormalizedUserName = s.ToLowerInvariant();
                })
                .Binding("Email", (contentItem, part, s) => part.Email = s)
                .Binding("Password", (contentItem, part, s) => {
                    part.HashAlgorithm = "SHA1";
                    _membershipService.SetPassword(part, s);
                });
        }
    }
}