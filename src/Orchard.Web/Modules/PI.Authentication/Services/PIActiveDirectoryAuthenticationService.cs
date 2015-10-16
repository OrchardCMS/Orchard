using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;

namespace PI.Authentication.Services
{
    public class PIActiveDirectoryAuthenticationService
    {

        private string _ldapString;

        public bool IsAuthenticated{get;private set;}

        public string Email { get; private set; }

        public List<string> MemberOf { get; private set; }

        public PIActiveDirectoryAuthenticationService(string ldapString, string domain, string userName, string password)
        {
            _ldapString = ldapString;
            AuthenticateUser(domain, userName, password);
        }

        private void AuthenticateUser(string domain, string userName, string password)
        {
            //Users are authenticated while login with active directory
            using (DirectoryEntry entry = new DirectoryEntry(_ldapString))
            {
                entry.Username = domain + "\\" + userName;
                entry.Password = password;

                //Bind to the native AdsObject to force authentication.
                object obj = entry.NativeObject;
               
                //Get Properties
                DirectorySearcher search = new DirectorySearcher(entry);
                search.PropertiesToLoad.Add("memberOf");
                search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("samAccountName");

                search.SearchRoot = entry;
                search.SearchScope = SearchScope.Subtree;
                search.Filter = string.Format("(&(samAccountName={0}))", userName);

                SearchResult result = null;
                try
                {
                    result = search.FindOne();
                    IsAuthenticated = result != null;
                }
                catch(Exception ex)
                {
                    IsAuthenticated = false;
                    return;
                }
                
                //Proceed if Authenticated
                if (IsAuthenticated)
                {
                    //Get Roles
                    DirectoryEntry user = result.GetDirectoryEntry();
                    if (user.Properties["mail"] != null)
                        Email = user.Properties["mail"].Value.ToString();

                    var g = user.Properties["memberOf"];
                    List<string> groupMembers = new List<string>();
                    foreach (string str in g)
                    {
                        string str2 = str.Substring(str.IndexOf("=") + 1, str.IndexOf(",") - str.IndexOf("=") - 1);
                        groupMembers.Add(str2);
                    }
                    MemberOf = groupMembers;
                }
            }
            
        }

    }
}