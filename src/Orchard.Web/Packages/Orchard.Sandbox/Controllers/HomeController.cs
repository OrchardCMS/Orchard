using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Sandbox.Models;
using Orchard.Sandbox.ViewModels;

namespace Orchard.Sandbox.Controllers
{
    public class Home : Controller {
        
        public ActionResult Index()
        {            
            return RedirectToAction("index","page");
        }

    }
}
