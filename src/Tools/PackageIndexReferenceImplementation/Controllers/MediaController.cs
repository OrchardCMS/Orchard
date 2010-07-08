using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PackageIndexReferenceImplementation.Services;

namespace PackageIndexReferenceImplementation.Controllers
{
    public class MediaController : Controller
    {  
        private readonly MediaStorage _mediaStorage;

        public MediaController() {
            _mediaStorage = new MediaStorage();
        }

        public ActionResult Resource(string id, string contentType)
        {
            return new StreamResult(contentType, _mediaStorage.GetMedia(id + ":" + contentType));
        }
    }
}
