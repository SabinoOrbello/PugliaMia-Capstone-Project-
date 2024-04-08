using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PugliaMia.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult BackOffice()
        {
            ViewBag.Title = "Back Office";

            return View();
        }
    }
}
