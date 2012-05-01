using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Sitecore.Pipelines.HttpRequest;

namespace BoC.Sitecore.Mvc
{
	public class MvcFixHttpProcessor : HttpRequestProcessor
	{
		public override void Process(HttpRequestArgs args)
		{
			//when using a path such as /Controller.aspx/Blahblahblah, Sitecore's parsing of FilePath can break if Blahblahblah is too long
			RouteData routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(args.Context));
			if (routeData != null && (routeData.DataTokens == null || 
                !SitecoreMvcAreaRegistration.SitecoreAreaName.Equals(routeData.DataTokens["area"])))
			{
				args.Url.FilePath = args.Context.Request.Url.LocalPath;
				args.Context.Items["SitecoreOn"] = false;
				args.AbortPipeline();
			}
		}
	}
}
