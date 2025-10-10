using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Users.Models;

namespace Orchard.Users.ViewModels {

    public class UsersIndexViewModel  {
        public IList<UserEntry> Users { get; set; }
        public UserIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class UserEntry {
        public UserEntry() {
            AdditionalActionLinks = new List<Func<HtmlHelper, MvcHtmlString>>();
        }

        public UserPart UserPart { get; set; }
        public UserPartRecord User { get; set; }
        public bool IsChecked { get; set; }
        public List<Func<HtmlHelper, MvcHtmlString>> AdditionalActionLinks { get; set; }
    }

    public class UserIndexOptions {
        public string Search { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public UsersBulkAction BulkAction { get; set; }
    }

    public enum UsersOrder {
        Name,
        Email,
        CreatedUtc,
        LastLoginUtc
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
