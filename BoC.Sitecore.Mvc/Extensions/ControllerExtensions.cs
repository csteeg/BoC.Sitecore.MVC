using System.Reflection;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.WebPages;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Links;
using Sitecore.SecurityModel;
using Sitecore.Web.UI.WebControls;

//hmm, it's the easiest way to use extensions, but not sure if it's ok to use the system.web.mvc namespace yet
namespace System.Web.Mvc
{
	public static class ControllerExtensions
	{
		public static ActionResult RedirectToItem(this IController controller, string path)
		{
            return new RedirectToRouteResult(new RouteValueDictionary() {{"Path", path}});
		}

        public static ActionResult RedirectToItem(this IController controller, Guid id)
        {
            return new RedirectToRouteResult(new RouteValueDictionary() { { "Id", id} });
        }

    }

}
