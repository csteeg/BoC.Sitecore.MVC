using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoC.Sitecore.Mvc.MvcHelpers;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Pipelines.LoadLayout;

namespace BoC.Sitecore.Mvc
{
    public class MvcAbortLoadLayout
    {
        public void Process(LoadLayoutArgs args)
        {
            Assert.ArgumentNotNull((object) args, "args");
            if (System.Web.HttpContext.Current != null && !String.IsNullOrEmpty(System.Web.HttpContext.Current.Request["id"]))
            {
                var id = new ID(System.Web.HttpContext.Current.Request["id"]);
                if (ControllerAction.GetControllerAction(MvcLayoutDataProvider.parentID, id) != null ||
                    ControllerAction.GetControllerAction(MvcSubLayoutDataProvider.parentId, id) != null)
                {
                    args.Body = args.Html = "It is not possible to design MVC layouts";
                    
                    args.AbortPipeline();
                }
            }
        }
    }
}
