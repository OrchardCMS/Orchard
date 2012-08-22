using System;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Tokens.Tests {
    public class TestTokenProvider : ITokenProvider {
        public TestTokenProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Site")
                .Token("Global1", T("Global1"), T("description of token1"))
                .Token("Global2", T("Global2"), T("description of token2"))
                .Token("Global3", T("Global3"), T("description of token3"))
                .Token("CurrentUser", T("Current User"), T("The current user"), "User");

            context.For("User")
                .Token("Name", T("Name"), T("Their user name"))
                .Token("Birthdate", T("Birthdate"), T("Date of birth"), "DateTime");

            context.For("Date")
                .Token("Now", T("Now"), T("Current system date in short date format. You can chain a .NET DateTime format string to customize."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<object>("Site", null)
                .Token("Global1", o => "[global1]")
                .Token("Global2", o => "[global2]")
                .Token("Global3", o => "[global3]")
                .Token("CurrentUser", o => new TestUser { UserName = "CurrentUser" })
                .Chain("CurrentUser", "User", o => new TestUser { UserName = "CurrentUser" });

            context.For<IUser>("User", () => new TestUser { UserName = "CurrentUser" })
                .Token("Name", u => u.UserName)
                .Token("Birthdate", u => "Nov 15")
                .Chain("Birthdate", "DateTime", u => new DateTime(1978, 11, 15));

            context.For<object>("Date", null)
                .Token("Now", o => DateTime.Now.ToShortDateString())
                .Chain("Now", "DateTime", o => DateTime.Now);

            context.For<DateTime>("DateTime")
                .Token((token, value) => value.ToString(token));
        }

    }

    public class TestUser : IUser {
        public string UserName { get; set; }
        public string Email { get; set; }
        public ContentItem ContentItem { get; set; }
        public int Id { get; set; }
    }
}
