using System;
using System.Web.Mvc;

namespace Orchard.Mvc.FollowReturnUrl {
    [AttributeUsage(AttributeTargets.Method)]
    public class FollowReturnUrlAttribute : FilterAttribute {
    }
}