using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Users.Models {
    public class UserDriver : ModelDriverWithRecord<UserRecord> {
        public UserDriver(IRepository<UserRecord> repository)
            : base(repository) {
        }

        protected override void New(NewModelContext context) {
            if (context.ModelType == "user") {
                context.Builder.Weld<UserModel>();
            }
        }
    }
}
