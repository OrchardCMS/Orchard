using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Tokens.Providers {
    public class UserTokens : ITokenProvider {
        private readonly IOrchardServices _orchardServices;
        private static readonly IUser _anonymousUser = new AnonymousUser();

        public UserTokens(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("User", T("User"), T("User tokens"))
                .Token("Name", T("Name"), T("Username"))
                .Token("Email", T("Email"), T("Email Address"))
                .Token("Id", T("Id"), T("User Id"))
                .Token("Content", T("Content"), T("The user's content item"));
        }

        public void Evaluate(EvaluateContext context) {
            context.For("User", () => _orchardServices.WorkContext.CurrentUser ?? _anonymousUser)
                .Token("Name", u => u.UserName)
                .Token("Email", u => u.Email)
                .Token("Id", u => u.Id)
                .Chain("Content", "Content", u => u.ContentItem);
            // todo: cross-module dependency -- should be provided by the User module?
            //.Token("Roles", user => string.Join(", ", user.As<UserRolesPart>().Roles.ToArray()));
        }

        public class AnonymousUser : IUser {
            public string UserName {
                get { return "Anonymous"; }
            }

            public string Email {
                get { return string.Empty; }
            }

            public ContentManagement.ContentItem ContentItem {
                get { return null; }
            }

            public int Id {
                get { return -1; }
            }
        }

    }
}