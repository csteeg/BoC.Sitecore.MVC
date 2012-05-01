using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BoC.Sitecore.Mvc.MvcHelpers
{
	internal class MvcActionHelper
	{
		internal static RouteData GetRouteData(
			HttpContextBase httpContext,
			string actionName, 
			string controllerName,
			RouteValueDictionary additionalRouteValues,
			bool isChildRequest,
			out VirtualPathData vpd)
		{
			if (String.IsNullOrEmpty(actionName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "actionName");
			}
			if (String.IsNullOrEmpty(controllerName))
			{
				throw new ArgumentException("Value cannot be null or empty.", "controllerName");
			}

			var routeData = RouteTable.Routes.GetRouteData(httpContext) ?? new RouteData();
			var routeValues = MergeDictionaries(additionalRouteValues, routeData.Values);

			routeValues["action"] = actionName;
			routeValues["controller"] = controllerName;
		    routeValues["area"] = SitecoreMvcAreaRegistration.SitecoreAreaName;

			vpd = RouteTable.Routes.GetVirtualPathForArea(new RequestContext(httpContext, routeData), null /* name */, routeValues);
			if (vpd == null)
			{
				throw new InvalidOperationException("No route in the route table matches the supplied values");
			}

			if (isChildRequest)
			{
				if (routeValues.ContainsKey("area"))
				{
					routeValues.Remove("area");
				}
				if (additionalRouteValues != null)
				{
					if (additionalRouteValues.ContainsKey("area"))
					{
						additionalRouteValues.Remove("area");
					}
					routeValues[ChildActionValuesKey] = new DictionaryValueProvider<object>(additionalRouteValues,
																							CultureInfo.InvariantCulture);
				}
			}

			return CreateRouteData(vpd.Route, routeValues, vpd.DataTokens);
		}

		private static string childActionValuesKey;

		protected static string ChildActionValuesKey
		{
			get
			{
				if (childActionValuesKey == null)
				{
					childActionValuesKey =
						typeof(ChildActionValueProvider)
							.GetProperty("ChildActionValuesKey", BindingFlags.Static | BindingFlags.NonPublic)
							.GetValue(null, null) as string;
				}
				return childActionValuesKey;
			}
		}

		private static RouteData CreateRouteData(RouteBase route, RouteValueDictionary routeValues, RouteValueDictionary dataTokens)
		{
			RouteData routeData = new RouteData();

			foreach (KeyValuePair<string, object> kvp in routeValues)
			{
				routeData.Values.Add(kvp.Key, kvp.Value);
			}

			foreach (KeyValuePair<string, object> kvp in dataTokens)
			{
				routeData.DataTokens.Add(kvp.Key, kvp.Value);
			}

			routeData.Route = route;
			routeData.DataTokens["ParentActionViewContext"] = new ViewContext();
			return routeData;
		}

		private static RouteValueDictionary MergeDictionaries(params RouteValueDictionary[] dictionaries)
		{
			// Merge existing route values with the user provided values
			var result = new RouteValueDictionary();

			foreach (RouteValueDictionary dictionary in dictionaries.Where(d => d != null))
			{
				foreach (KeyValuePair<string, object> kvp in dictionary)
				{
					if (!result.ContainsKey(kvp.Key))
					{
						result.Add(kvp.Key, kvp.Value);
					}
				}
			}

			return result;
		}
	}
}
