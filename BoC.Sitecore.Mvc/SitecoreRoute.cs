using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BoC.Sitecore.Mvc.MvcHelpers;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.SecurityModel;
using Sitecore.Web;

namespace BoC.Sitecore.Mvc
{
    public class SitecoreRoute: Route
    {
        internal SitecoreRoute(): base("{*path}", new MvcRouteHandler()){}

        static LayoutItem GetQueryStringLayout()
        {
            string queryString = WebUtil.GetQueryString("sc_layout");
            if (string.IsNullOrEmpty(queryString))
                return (LayoutItem)null;
            else
                return (LayoutItem)Context.Database.GetItem(queryString);
        }
        
        public override RouteData GetRouteData(System.Web.HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext) ?? new RouteData(this, this.RouteHandler);
            //if (routeData != null && routeData.Values.ContainsKey("id"))
            {
                //new ItemResolver().
                var item = Context.Item;//(Context.Database ?? Context.ContentDatabase).GetItem(Context.Site.RootPath + routeData.Values["id"]);
                if (item != null)
                {
                    var layoutItem = GetQueryStringLayout() ?? Context.Item.Visualization.Layout;
                    if (layoutItem != null)
                    {
                        var action = ControllerAction.GetControllerAction(MvcLayoutDataProvider.parentId, layoutItem.ID)
                                     ?? ControllerAction.GetControllerAction(MvcSubLayoutDataProvider.parentId, layoutItem.ID);
                        if (action != null)
                        {
                            routeData.Values["action"] = action.ActionName;
                            routeData.Values["controller"] = action.ControllerType.ControllerName;
                            routeData.Values["_sitecoreitem"] = item;
                            routeData.Values["_sitecoremvcaction"] = action;
                            var wildcardItem = item;
                            var i = 0;
                            using (new SecurityDisabler())
                            {
                                while (wildcardItem != null)
                                {
                                    if (wildcardItem.Name == "*" && wildcardItem.DisplayName.Length > 1)
                                    {
                                        string name = wildcardItem.DisplayName.TrimStart('*', '(', ' ').TrimEnd(')', ' ');
                                        if (!routeData.Values.ContainsKey(name))
                                        {
                                            routeData.Values.Add(name, WebUtil.GetUrlName(i));
                                        }
                                    }
                                    wildcardItem = wildcardItem.Parent;
                                    i++;
                                }
                            }
                            return routeData;
                        }
                    }
                }
            }
            return null;
        }
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var usedValues = new List<string> { "_sitecoreitem" , "item"  };
            var item = values["_sitecoreitem"] as Item ?? values["item"] as Item;
            var requestedPath = new string[0];
            if (item == null)
            {
                var id = values["path"] ?? values["id"];
                usedValues.Add(id == values["path"] ? "path" : "id");
                if (id != null)
                {
                    if (id is string && (id.ToString()).Contains("/"))
                    {
                        if (!(id.ToString()).StartsWith(Context.Site.RootPath))
                            id = Context.Site.RootPath + "/" + id.ToString().TrimStart('/');
                        requestedPath = id.ToString().Split(new[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
                    }
                    if (id is ID)
                    {
                        item = (Context.Database ?? Context.ContentDatabase).GetItem((ID)id);
                    }
                    else
                    {
                        item = (Context.Database ?? Context.ContentDatabase).GetItem(id.ToString());
                    }
                }
            }
            if (item != null)
            {
                var url = LinkManager.GetItemUrl(item);
                url = VirtualPathUtility.ToAppRelative(url);
                if (url.StartsWith("~/"))
                    url = url.Substring(2);

                var urlparts = url.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                var wildcardItem = item;
                var i = urlparts.Length - 1;
                using (new SecurityDisabler())
                {
                    while (wildcardItem != null && i >= 0)
                    {
                        if (wildcardItem.Name == "*" && wildcardItem.DisplayName.Length > 1)
                        {
                            string name = wildcardItem.DisplayName.TrimStart('*', '(', ' ').TrimEnd(')', ' ');
                            if (values.ContainsKey(name))
                            {
                                urlparts[i] = values[name] + "";
                            }
                            else
                            {
                                if (i < requestedPath.Length)
                                {
                                    //requestedpath is always the full path!
                                    urlparts[i] =
                                        requestedPath[
                                            wildcardItem.Paths.FullPath.Split(new[] {'/'},
                                                                              StringSplitOptions.RemoveEmptyEntries).
                                                Length - 1];
                                }
                            }
                            usedValues.Add(name.ToLower());
                        }
                        wildcardItem = wildcardItem.Parent;
                        i--;
                    }
                }
                var newurl = new StringBuilder(string.Join("/", urlparts));
                var extension = Path.GetExtension(url);
                if (!string.IsNullOrEmpty(extension) && !extension.Equals(Path.GetExtension(newurl.ToString())))
                    newurl.Append(extension);

                // Add remaining new values as query string parameters to the URL 
                // Generate the query string
                bool firstParam = !newurl.ToString().Contains("?");
                var unusedValues = values.Where(v => !usedValues.Contains(v.Key.ToLower()));
                foreach (var unusedValue in unusedValues)
                {
                    newurl.Append(firstParam ? '?' : '&');
                    firstParam = false;
                    newurl.Append(Uri.EscapeDataString(unusedValue.Key));
                    newurl.Append('=');
                    newurl.Append(Uri.EscapeDataString(System.Convert.ToString(unusedValue.Value, CultureInfo.InvariantCulture)));
                }

                return new VirtualPathData(this, newurl.ToString());
            }

            return null;
        }
    }

}
