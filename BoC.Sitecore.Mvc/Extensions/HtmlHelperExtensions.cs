using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.WebPages;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines;
using Sitecore.Pipelines.RenderLayout;
using Sitecore.Web.UI.WebControls;

namespace BoC.Sitecore.Mvc.Extensions
{
	public static class HtmlHelperExtensions
	{
		public static HelperResult RenderPlaceHolder(this HtmlHelper html, string key)
		{
			return new HelperResult(
				writer => 
					new SitecorePlaceholder(key).RenderView(
						new ViewContext(html.ViewContext, html.ViewContext.View, html.ViewData, html.ViewContext.TempData, writer)
					)
			);
		}
	}

	class SitecorePlaceholder : ViewUserControl
	{
		private readonly string key;

		public SitecorePlaceholder(string key)
		{
			this.key = key;
		}

		public override void RenderView(ViewContext viewContext)
		{
			var prevHandler = this.Context.Handler;
			var isOnAspx = prevHandler is Page;

			this.Controls.Add(new Placeholder() { Key = key });
			if (!isOnAspx)
			{
				this.Controls.Add(new SitecoreForm());
			}
			using (var containerPage = new PageHolderContainerPage(this))
			{
				try
				{
					if (!isOnAspx)
						this.Context.Handler = containerPage;
					if (global::Sitecore.Context.Page == null)
					{
						viewContext.Writer.WriteLine("<!-- Unable to use sitecoreplacholder outside sitecore -->");
						return;
					}
					InitializePageContext(containerPage, viewContext);
					RenderViewAndRestoreContentType(containerPage, viewContext);
				}
				finally
				{
					this.Context.Handler = prevHandler;
				}
			}
		}

		internal static MethodInfo pageContextInitializer = typeof(PageContext).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
		internal static MethodInfo pageContextOnPreRender = typeof(PageContext).GetMethod("OnPreRender", BindingFlags.NonPublic | BindingFlags.Instance);
		internal static FieldInfo pageContext_page = typeof(PageContext).GetField("_page", BindingFlags.NonPublic | BindingFlags.Instance);
		internal static void InitializePageContext(Page containerPage, ViewContext viewContext)
		{

			PageContext pageContext = global::Sitecore.Context.Page;
			if (pageContext == null)
				return;

			var exists = pageContext.Renderings != null && pageContext.Renderings.Count > 0;
			if (!exists)
			{
				//use the default initializer:
				pageContextInitializer.Invoke(pageContext, null);
				//viewContext.HttpContext.Items["_SITECORE_PLACEHOLDER_AVAILABLE"] = true;
			}
			else
			{
				//our own initializer (almost same as Initialize in PageContext, but we need to skip buildcontroltree, since that is already availabe)
				pageContext_page.SetValue(pageContext, containerPage);
				containerPage.PreRender += (sender, args) => pageContextOnPreRender.Invoke(pageContext, new[] {sender, args});
				switch (Settings.LayoutPageEvent)
				{
					case "preInit":
						containerPage.PreInit += (o, args) => pageContext.Build();
						break;
					case "init":
						containerPage.Init += (o, args) => pageContext.Build();
						break;
					case "load":
						containerPage.Load += (o, args) => pageContext.Build();
						break;
				}
			}
		}

		internal static void RenderViewAndRestoreContentType(ViewPage containerPage, ViewContext viewContext) { 
            // We need to restore the Content-Type since Page.SetIntrinsics() will reset it. It's not possible
            // to work around the call to SetIntrinsics() since the control's render method requires the
            // containing page's Response property to be non-null, and SetIntrinsics() is the only way to set
            // this. 
            string savedContentType = viewContext.HttpContext.Response.ContentType;
            containerPage.RenderView(viewContext); 
            viewContext.HttpContext.Response.ContentType = savedContentType; 
        }


	    internal sealed class PageHolderContainerPage : ViewPage
		{
			[ThreadStatic]
			private static int _nextId = 0;
			
			private readonly ViewUserControl _userControl;

			public PageHolderContainerPage(ViewUserControl userControl)
			{
				_userControl = userControl;
			}

			public override void ProcessRequest(HttpContext context)
			{
				_userControl.ID = ID + "_" + (++_nextId);
				Controls.Add(_userControl);

				base.ProcessRequest(context);
			}
		}
	}

	class SitecoreForm : HtmlForm
	{
		protected override void AddedControl(Control control, int index)
		{
			base.AddedControl(control, index);
			var reference = global::Sitecore.Context.Page.GetRenderingReference(control);
			reference.AddedToPage = true;
		}

		protected override void Render(HtmlTextWriter output)
		{
			if (Controls.Count > 0)
				base.Render(output);
		}
	}
}
