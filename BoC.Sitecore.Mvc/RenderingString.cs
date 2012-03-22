using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Sitecore.Data.Items;
using Sitecore.Web.UI.WebControls;

namespace BoC.Sitecore.Mvc
{
    public class RenderingString: IHtmlString
    {
        private readonly string _value;
        private readonly Item _item;
        private readonly string _fieldName;

        public RenderingString()
        {
        }

        private RenderingString(string value)
        {
            _value = value;
        }

        public RenderingString(Item item, string fieldName)
        {
            _item = item;
            _fieldName = fieldName;
        }

        class FakePage : Page
        {
            static FieldInfo _request = typeof(Page).GetField("_request", BindingFlags.NonPublic | BindingFlags.Instance);
            private IHttpHandler _prevHandler;

            public FakePage()
            {
                if (HttpContext.Current == null || HttpContext.Current.Handler is Page)
                    return;
                
                _prevHandler = HttpContext.Current.Handler;
                HttpContext.Current.Handler = this;
                _request.SetValue(this, HttpContext.Current.Request);
            }

            public override void Dispose()
            {
                if (HttpContext.Current != null && _prevHandler != null)
                {
                    HttpContext.Current.Handler = _prevHandler;
                }
                base.Dispose();

            }
        }
        public string ToHtmlString()
        {
            if (_value != null)
            {
                return new HtmlString(_value).ToHtmlString();
            }
            //pff really stupid: UIUtil from sitecore requests the Page.request through HttpContext.current.handler
            //why not httpcontext.current.request? :(
            //we'll have to fake a page now:
            using (new FakePage())
                return FieldRenderer.Render(_item, _fieldName);
        }

        public override string ToString()
        {
            if (_value != null)
            {
                return new HtmlString(_value).ToString();
            }
            return _item[_fieldName];
        }

        public static implicit operator string(RenderingString value)
        {
            return value == null ? null : value.ToString();
        }

        public static implicit operator RenderingString(string value)
        {
            return new RenderingString(value);
        }

    }
}
