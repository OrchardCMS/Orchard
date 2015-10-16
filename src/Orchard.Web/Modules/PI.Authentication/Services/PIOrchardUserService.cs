using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PI.Authentication.Services
{
    public class PIOrchardUserService : IPIOrchardUserService
    {
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly IRepository<UserRolesPartRecord> _userPartRecordRepository;
        private readonly IAuthenticationService _authenticationService;


        public PIOrchardUserService(IAuthenticationService authenticationService, IRoleService roleService, IRepository<UserRolesPartRecord> userPartRecordRepository, IMembershipService membershipService)
        {
            _userPartRecordRepository = userPartRecordRepository;
            _roleService = roleService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void SyncUserToOrchardUser(string userName, string password, string email, bool signin)
        {
            //Enter user in Orchard db
            var orchardUser = _membershipService.GetUser(userName);

            if (orchardUser == null)  // new to orchard:  No user was found.
            {
                var user = _membershipService.CreateUser(
                    new CreateUserParams(
                        userName,
                        password,
                        email,
                        isApproved: true,
                        passwordQuestion: null,
                        passwordAnswer: null));

                //Assign role to Orchard User
                var roleRecord = _roleService.GetRoleByName("Authenticated");
                if (roleRecord != null)
                {
                    _userPartRecordRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = roleRecord });
                }
                else
                {
                    Logger.Debug("Role not found: {0}", "Authenticated");
                }
            }
            else  //User was found but now check if password is the same.
            {
                //Verify if the password is still valid in Orchard.  If not then change it to the one passed in.
                IUser verifyPW = _membershipService.ValidateUser(userName, password);
                if (verifyPW == null)
                {
                    verifyPW = _membershipService.GetUser(userName);
                    if (verifyPW != null)
                        _membershipService.SetPassword(verifyPW, password);

                    if(signin)
                        _authenticationService.SignIn(verifyPW, true);
                }


            }
        }
    }
}