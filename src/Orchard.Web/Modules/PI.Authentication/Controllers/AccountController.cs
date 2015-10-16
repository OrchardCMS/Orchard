using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using PI.Authentication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Mvc;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Extensions;
using Orchard.Users.Services;
using Orchard;
using Orchard.Users.Events;
using PI.Authentication.Models;
using Orchard.ContentManagement;
using Orchard.Themes;

namespace PI.Authentication.Controllers
{
    [Themed]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserEventHandler _userEventHandler;

        public AccountController(
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            IUserService userService,
            IOrchardServices orchardServices,
            IUserEventHandler userEventHandler)
        {
            //_authenticationService = authenticationService;
            _membershipService = membershipService;
            _userService = userService;
            _orchardServices = orchardServices;
            _userEventHandler = userEventHandler;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        // GET: Account
        public ActionResult Index()
        {
            var piAuthSettings = _orchardServices.WorkContext.CurrentSite.As<PIAuthenticationSettingsPart>();
            var loginPage = piAuthSettings.LoginPage;
            if(String.IsNullOrEmpty(loginPage))
            {
                throw new ApplicationException("PI Authentication: Login page was not set.");
            }


            string qs = Request.Url.Query;
            loginPage = loginPage + "{0}";
            string redirectLogin = String.Format(loginPage, qs ?? "");

            return Redirect(redirectLogin);
        }
        public ActionResult LogOff(string returnUrl)
        {
            _authenticationService.SignOut();

            return this.RedirectLocal(returnUrl);
        }
    }
}