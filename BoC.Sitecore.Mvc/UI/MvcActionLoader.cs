﻿using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using BoC.Sitecore.Mvc.MvcHelpers;
using Sitecore.Web;
using Sitecore.Web.UI;

namespace BoC.Sitecore.Mvc.UI
{
	public class MvcActionLoader : WebControl
	{
		public string Controller { get; set; }
		public string Action { get; set; }
		protected override void DoRender(HtmlTextWriter output)
		{
			var rendering = global::Sitecore.Context.Page.GetRenderingReference(this);
			if (rendering == null)
				return;

			var action = ControllerAction.GetControllerAction(MvcSubLayoutDataProvider.parentId, rendering.RenderingID);
			if (action == null)
				return;

            var httpContext = new HttpContextWrapper(Context);
            var existingRouteData = RouteTable.Routes.GetRouteData(httpContext);
            var additionalRouteValues = existingRouteData == null ? new RouteValueDictionary() : existingRouteData.Values ;
            if (!string.IsNullOrEmpty(DataSource))
            {
                var item = rendering.Database.GetItem(DataSource);
                if (item != null)
                {
                    additionalRouteValues["_sitecoreitem"] = item;
                }
            }
            additionalRouteValues["_sitecorerendering"] = rendering;
			var parameters = WebUtil.ParseUrlParameters(Parameters);
			foreach (var key in parameters.AllKeys)
			{
				additionalRouteValues[key] = parameters[key];
			}

			var routeData = MvcActionHelper.GetRouteData(
				httpContext,
				action.ActionName,
				action.ControllerType.ControllerName,
				additionalRouteValues,
				true
				);
			var handler = new MvcHandler(new RequestContext(httpContext, routeData));
			httpContext.Server.Execute(HttpHandlerUtil.WrapForServerExecute(handler), output, true /* preserveForm */);

		}
	}


}
