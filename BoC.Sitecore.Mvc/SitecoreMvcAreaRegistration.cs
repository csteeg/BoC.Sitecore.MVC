using System.Web.Mvc;

namespace BoC.Sitecore.Mvc
{
    public class SitecoreMvcAreaRegistration : AreaRegistration
    {
        public const string SitecoreAreaName = "sitecoremvc";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            context.Namespaces.Clear();
            context.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {id = UrlParameter.Optional}
                //do not set default controller & action, it's always needed since we load through sitecore
                //new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );
        }

        public override string AreaName
        {
            get { return SitecoreAreaName; }
        }
    }

}
