using System.Web.Mvc;

namespace BoC.Sitecore.Mvc.MvcHelpers
{
    public class SitecoreFilterAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext != null && (filterContext.Result is RedirectResult ||
                filterContext.Result is RedirectToRouteResult) && filterContext.RouteData.DataTokens.ContainsKey("ParentActionViewContext"))
            {
                //grow up fast, we want redirects from sitecore controls
                //filterContext.IsChildAction looks if "ParentActionViewContext" exists in datatokens;
                filterContext.RouteData.DataTokens.Remove("ParentActionViewContext");
            }
            base.OnResultExecuting(filterContext);
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.RouteData.DataTokens.ContainsKey("ParentActionViewContext"))
            {
                //grow up fast, we want errorhandling inside sitecore controls
                //filterContext.IsChildAction looks if "ParentActionViewContext" exists in datatokens;
                filterContext.RouteData.DataTokens.Remove("ParentActionViewContext");
            }

        }
    }
}
