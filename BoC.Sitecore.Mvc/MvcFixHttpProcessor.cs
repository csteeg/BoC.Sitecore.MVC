using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Sitecore;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Security.Accounts;
using Sitecore.Sites;

namespace BoC.Sitecore.Mvc
{
	public class MvcFixHttpProcessor : ExecuteRequest
	{
		public override void Process(HttpRequestArgs args)
		{
			//when using a path such as /Controller.aspx/Blahblahblah, Sitecore's parsing of FilePath can break if Blahblahblah is too long
			RouteData routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(args.Context));
			if (routeData != null)
                //!SitecoreMvcAreaRegistration.SitecoreAreaName.Equals(routeData.DataTokens["area"])))
			{
                args.Url.FilePath = args.Context.Request.Url.LocalPath;
                if (!(routeData.Route is SitecoreRoute))
                {
                    args.Context.Items["SitecoreOn"] = false;
                    args.AbortPipeline();
                }
                else
                {
                    SiteContext site = Context.Site;
                    if (site != null && !SiteManager.CanEnter(site.Name, (Account)Context.User))
                    {
                        base.Process(args);
                    }
                }
			}
			else
			{
			    base.Process(args);
			}
		}
	}
}
