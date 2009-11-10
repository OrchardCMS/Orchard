using Orchard.Models.Driver;

namespace Orchard.Users.Models {
    public class UserDriver : ModelDriver {
        protected override void New(NewModelContext context) {
            if (context.ModelType == "user") {
                WeldModelPart<UserModel>(context);
            }
        }
    }
}
