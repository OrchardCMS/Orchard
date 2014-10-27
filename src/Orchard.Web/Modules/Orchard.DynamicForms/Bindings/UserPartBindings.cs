using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Users.Models;

namespace Orchard.DynamicForms.Bindings {
    public class UserPartBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<UserPart>()
                .Binding("UserName", (part, s) => part.UserName = s)
                .Binding("Email", (part, s) => part.Email = s)
                .Binding("Password", (part, s) => part.Password = s);
        }
    }
}