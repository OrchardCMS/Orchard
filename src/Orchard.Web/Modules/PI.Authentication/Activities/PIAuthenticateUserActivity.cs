using Orchard;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using PI.Authentication.Models;
using PI.Authentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using Orchard.ContentManagement;
using System.Web.Mvc;


namespace PI.Authentication.Activities
{
    //[OrchardFeature("PI.UserAuthentication.Workflows")]
    public class PIAuthenticateUserActivity : Task
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IPIOrchardUserService _orchardUserService;

        private readonly LocalizedString LOGIN_ASP_SUCCESS;
        private readonly LocalizedString LOGIN_LDAP_SUCCESS;
        private readonly LocalizedString LOGIN_FAILED;
        private readonly LocalizedString LOGIN_REQUIRED_USERNAME;
        private readonly LocalizedString LOGIN_REQUIRED_PASSWORD;
        private readonly LocalizedString LOGIN_ABORTED_ALREADYLOGGEDIN;



        //private PIUser PI_User = new PIUser();
        public PIAuthenticateUserActivity(IAuthenticationService authenticationService, IPIOrchardUserService orchardUserService, IRoleService roleService, IRepository<UserRolesPartRecord> userPartRecordRepository, IMembershipService membershipService, IOrchardServices orchardServices, IUserEventHandler userEventHandler)
        {
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _userEventHandler = userEventHandler;
            T = NullLocalizer.Instance;

            _authenticationService = authenticationService;
            _orchardUserService = orchardUserService; 

            LOGIN_ASP_SUCCESS = T("Successful ASPMembership Login");
            LOGIN_LDAP_SUCCESS = T("Successul LDAP Login");
            LOGIN_FAILED = T("Failed Login: Authentication Failed");
            LOGIN_REQUIRED_USERNAME = T("Failed Login: Username is required");
            LOGIN_REQUIRED_PASSWORD = T("Failed Login: Password is required");
            LOGIN_ABORTED_ALREADYLOGGEDIN = T("Aborted Login: User is already logged in");
        }

        public Localizer T { get; set; }

        public override string Name
        {
            get { return "PI_User_Authentication"; }
        }

        public override LocalizedString Category
        {
            get { return T("User"); }
        }

        public override LocalizedString Description
        {
            get { return T("Authenticate PI User based on the specified values."); }
        }

        public override string Form
        {
            get { return Globals.Forms.PIAuthenticateUserForm; }
        }
        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] {
                LOGIN_FAILED,
                LOGIN_LDAP_SUCCESS,
                LOGIN_ASP_SUCCESS,
                LOGIN_REQUIRED_USERNAME,
                LOGIN_REQUIRED_PASSWORD,
                LOGIN_ABORTED_ALREADYLOGGEDIN,
            };
        }

        [OutputCache(Duration=0)]
        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var loginString = activityContext.GetState<string>("UserName");
            var loginPassword = activityContext.GetState<string>("Password");
            var rememberMe = activityContext.GetState<string>("RememberMe");


            //Check if user is already logged in
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if(currentUser != null)
            {
                yield return LOGIN_ABORTED_ALREADYLOGGEDIN;
                yield break;
            }

            //Validate login fields
            if (string.IsNullOrWhiteSpace(loginString))
            {
                yield return LOGIN_REQUIRED_USERNAME;
                yield break;
            }
            if (string.IsNullOrWhiteSpace(loginPassword))
            {
                yield return LOGIN_REQUIRED_PASSWORD;
                yield break;
            }


            //local properties
            LocalizedString retVal = LOGIN_FAILED;
            MembershipUser aspMembership = null;
            PIActiveDirectoryAuthenticationService ldapAuth = null;
            string userName = null;
            string password = loginPassword;
            string email = null;
            string domain = null;

            //Check if login user is domain user using active directory
            if (loginString.Contains("\\"))
            {
                string[] loginStringSplit = loginString.Split('\\');
                if (loginStringSplit.Count() == 2)
                {
                    domain = loginStringSplit[0];
                    userName = loginStringSplit[1];
                   
                    //Get ldap settings
                    var piAuthSettings = _orchardServices.WorkContext.CurrentSite.As<PIAuthenticationSettingsPart>();
                    var ldapStringSetting = piAuthSettings.LDAPString;
                    var domainStringSetting = piAuthSettings.Domain;

                    //if correct domain check continue to authenticate in active directory
                    if (domain == domainStringSetting)
                    {
                        ldapAuth = new PIActiveDirectoryAuthenticationService(ldapStringSetting, domain, userName, password);
                        
                        if (ldapAuth.IsAuthenticated)
                        {
                            userName = loginString;
                            email = ldapAuth.Email;
                            retVal = LOGIN_LDAP_SUCCESS;
                        }
                    }
                }
            }
            else if (Regex.IsMatch(loginString ?? "", Globals.EmailPattern, RegexOptions.IgnoreCase))   //ASP Membership authentication (loginString is in email format)
            {
                userName = loginString;  //This will be an email format username
                email = userName;

                //Validate login using Asp.net Membership using email as username 
                var isUserInMembership = Membership.ValidateUser(userName, password);

                //If initial authentication failed try again using aspMem username after extracting email address
                string aspMembershipUserName = null;
                if(!isUserInMembership)
                {
                    aspMembershipUserName = Membership.GetUserNameByEmail(email);
                    isUserInMembership = Membership.ValidateUser(aspMembershipUserName, password);
                }

                if(isUserInMembership)
                {
                    userName = loginString;
                    aspMembership = Membership.GetUser(aspMembershipUserName);
                    retVal = LOGIN_ASP_SUCCESS;
                }
               
            }

            //All attempts to authenticate with external user mgt should be done by now let's turn to Orchard User Mgt.
            if (retVal == LOGIN_LDAP_SUCCESS || retVal == LOGIN_ASP_SUCCESS)
            {
                //Sync User in Orchard if necessary. User will be created if absent.  Otherwise Orchard password will be updated if different.
                _orchardUserService.SyncUserToOrchardUser(userName, password, email, true);
               

                //Authenticate user in Orchard. ( Wow! double authentication )
                IUser verifyPW = _membershipService.ValidateUser(userName, password);
                if (verifyPW != null)
                    _authenticationService.SignIn(verifyPW, true);
                else
                    retVal = LOGIN_FAILED;
            }


            yield return retVal;
            yield break;
        }
    }
}