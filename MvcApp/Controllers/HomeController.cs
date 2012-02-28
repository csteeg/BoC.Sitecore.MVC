using System.ComponentModel;
using System.Web.Mvc;
using MvcTestApp.Models;

namespace MvcTestApp.Controllers
{
	//[Description("My {0}")]
    public class HomeController : Controller
    {
        public ActionResult Index(ItemView item)
        {
            return View(item);
        }
	}
}
