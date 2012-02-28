using System;
using System.Web.Mvc;
using BoC.Sitecore.Mvc.MvcHelpers;
using Sitecore;
using Sitecore.Pipelines;
using Sitecore.Web.UI.WebControls;

namespace BoC.Sitecore.Mvc.Initialize
{
    public class RegisterValueProvider
    {
        public void Process(PipelineArgs args)
        {
            ValueProviderFactories.Factories.Add(
                    new SitecoreValueProviderFactory()
                );
        }
    }

}
