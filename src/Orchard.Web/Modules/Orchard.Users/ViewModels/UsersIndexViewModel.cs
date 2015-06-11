using System.Collections.Generic;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {

    public class UsersIndexViewModel  {
        public IList<UserEntry> Users { get; set; }
        public UserIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class UserEntry {
        public UserPartRecord User { get; set; }
        public bool IsChecked { get; set; }
    }

    public class UserIndexOptions {
        public string Search { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public UsersBulkAction BulkAction { get; set; }
    }

    public enum UsersOrder {
        Name,
        Email
    }

    public enum UsersFilter {
        All,
        Approved,
        Pending,
        EmailPending
    }

    public enum UsersBulkAction {
        None,
        Delete,
        Disable,
        Approve,
        ChallengeEmail
    }
}
