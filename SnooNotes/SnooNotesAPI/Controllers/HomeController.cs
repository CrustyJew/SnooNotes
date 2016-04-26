using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SnooNotesAPI.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			ViewBag.Title = "SnooNotes";

			return View();
		}
        public ActionResult GetTheAwesome()
        {
            if (Request.Browser.Browser == "Chrome")
            {
                return Redirect("https://chrome.google.com/webstore/detail/snoonotes/lfoenkalfeojpdlgiccblfbjcjpanneg");
            }
            else if (Request.Browser.Browser == "Firefox")
            {
                //return File("/Addon/snoonotes.xpi", "application/x-xpinstall");
                return Redirect( "https://addons.mozilla.org/en-US/firefox/addon/snoonotes/" );
            }
            else
            {
                return View("UnrecognizedBrowser");
            }
        }
	}
}
