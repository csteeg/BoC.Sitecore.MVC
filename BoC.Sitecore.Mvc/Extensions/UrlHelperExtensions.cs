using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Links;

namespace BoC.Sitecore.Mvc.Extensions
{

    //public static class UrlHelperExtensions
    //{
    //    public static HelperResult RouteItemUrl(this UrlHelper url, string path)
    //    {
    //        return new HelperResult(
    //            writer =>
    //                {
    //                    var item = (Context.Database ?? Factory.GetDatabase("master")).GetItem(path);
    //                    if (item != null)
    //                        writer.Write(LinkManager.GetItemUrl(item));
    //                }
    //        );
    //    }
    //    public static HelperResult RouteItemUrl(this UrlHelper url, Guid id)
    //    {
    //        return new HelperResult(
    //            writer =>
    //            {
    //                var item = (Context.Database ?? Factory.GetDatabase("master")).GetItem(new ID(id));
    //                if (item != null)
    //                    writer.Write(LinkManager.GetItemUrl(item));
    //            }
    //        );
    //    }
    //}

}
