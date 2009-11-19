using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Orchard.Data;
using Orchard.DevTools.ViewModels;
using Orchard.Models;
using Orchard.Models.Records;
using Orchard.Mvc.ViewModels;

namespace Orchard.DevTools.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new BaseViewModel());
        }

    }
}
