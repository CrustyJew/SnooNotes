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

            return File("/Addon/snoonotes.xpi", "application/x-xpinstall");
        }
	}
}
