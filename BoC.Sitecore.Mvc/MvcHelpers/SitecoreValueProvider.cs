using System;
using System.Web.Mvc;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace BoC.Sitecore.Mvc.MvcHelpers
{
    public class SitecoreValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            return new SitecoreValueProvider(controllerContext);

        }
    }
    
    public class SitecoreValueProvider : IValueProvider
    {
        private readonly ControllerContext _controllerContext;

        public SitecoreValueProvider(ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
        }

        public bool ContainsPrefix(string prefix)
        {
            return "item".Equals(prefix) || prefix.StartsWith("item.") || prefix.StartsWith("item[");
        }

        public ValueProviderResult GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (key.StartsWith("item."))
                key = key.Substring("item.".Length);

            Item item = null;
            if (_controllerContext.RouteData.Values.ContainsKey("id"))
            {
                Guid id;
                if (Guid.TryParse(_controllerContext.RouteData.Values["id"] as string, out id))
                {
                    item = global::Sitecore.Context.Database.GetItem(new ID(id));
                }
                else
                {
                    item = global::Sitecore.Context.Database.GetItem(_controllerContext.RouteData.Values["id"] + "");
                }
            }
            if (item == null)
            {
                item = _controllerContext.RouteData.Values["item"] as Item ?? _controllerContext.RouteData.Values["_sitecoreitem"] as Item ?? global::Sitecore.Context.Item;
            }
            if (item == null)
            {
                return null;
            }
            if (key == "Name" || key == "DisplayName")
            {
                return new ValueProviderResult(key == "Name" ? item.Name : item.DisplayName, key == "Name" ? item.Name : item.DisplayName, item.Language.CultureInfo);
            }

            Field field = null;
            if ((field = item.Fields[key]) == null)
                return null;
            
            return new ValueProviderResult(new RenderingString(item, key), field.Value, item.Language.CultureInfo);
        }
    }
}