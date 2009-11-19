using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Wikis.Models;
using Orchard.Wikis.ViewModels;

namespace Orchard.Wikis.Controllers
{
    public class Home : Controller {
        
        public ActionResult Index()
        {            
            return RedirectToAction("index","page");
        }

    }
}
