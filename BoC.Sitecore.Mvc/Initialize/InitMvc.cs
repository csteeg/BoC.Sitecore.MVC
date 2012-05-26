using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using BoC.Sitecore.Mvc.MvcHelpers;
using Sitecore;
using Sitecore.Pipelines;
using Sitecore.Web.UI.WebControls;

namespace BoC.Sitecore.Mvc.Initialize
{
    public class InitMvc
    {
        public void Process(PipelineArgs args)
        {
            //at the very beginnen, we trigger the session to have an Id.
            //for some reason we'll get an error that the sessionId can't be created otherwise.
            var module = HttpContext.Current.ApplicationInstance.Modules["Session"] as SessionStateModule;
            if (module != null)
            {
                module.Start += (sender, eventArgs) => { var sessionId = HttpContext.Current.Session.SessionID; };
            }
            ValueProviderFactories.Factories.Add(new SitecoreValueProviderFactory());
            AreaRegistration.RegisterAllAreas();
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            RouteTable.Routes.Insert(0, new SitecoreRoute());
        }
    }

}
