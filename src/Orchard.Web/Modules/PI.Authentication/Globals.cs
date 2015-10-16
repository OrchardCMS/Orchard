using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PI.Authentication
{
    public class Globals
    {
        public const string EmailPattern =
        @"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
        + @"@([a-z0-9][\w-]*\.)+[a-z]{2,}$";

        public struct WorkflowParam
        {
            public static string UserName = "Username";
            public static string Password = "Password";
        }
        public struct Forms
        {
            public static string PIAuthenticateUserForm = "PIAuthenticateUserForm";
        }

    }
}