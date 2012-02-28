using System.Linq;
using System.Web;
using BoC.Sitecore.Mvc.MvcHelpers;
using BoC.Sitecore.Mvc.UI;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.DataProviders;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;

namespace BoC.Sitecore.Mvc
{
	public class MvcSubLayoutDataProvider : MvcLayoutDataProvider
	{
		private readonly ID masterId = ID.Null;
		protected override ID MasterId { get { return masterId; } }
		private readonly ID baseTemplateId = new ID("{1DDE3F02-0BD7-4779-867A-DC578ADF91EA}");
		protected override ID BaseTemplateId { get { return baseTemplateId; } }
		internal static ID parentId = ItemIDs.LayoutRoot;
		protected override ID ParentId { get { return parentId; } }
		private readonly ID folderTemplateId = new ID("{3BAA73E5-6BA9-4462-BF72-C106F8801B11}"); ///sitecore/templates/System/Layout/Renderings/Sublayout Folder - 
		protected override ID FolderTemplateId { get { return folderTemplateId; } }
		private readonly ID folderId = new ID("{11111112-1EE3-4181-A7E8-DFC489EAB2C4}");
		protected override ID FolderId { get { return folderId; } }
		protected override string FolderName { get { return "MVC Actions"; } }

		public override FieldList GetItemFields(ItemDefinition item, VersionUri version, CallContext context)
		{
			Assert.ArgumentNotNull(item, "item");
			Assert.ArgumentNotNull(version, "version");
			Assert.ArgumentNotNull(context, "context");

			var action = ControllerAction.GetControllerAction(ParentId, item.ID);
			if (action == null)
			{
				return base.GetItemFields(item, version, context);
			}

			FieldList list = new FieldList();
			var template = context.DataManager.Database.GetTemplate(BaseTemplateId);
			list.Add(template.GetField("Namespace").ID, typeof(MvcActionLoader).Namespace);
			list.Add(template.GetField("Tag").ID, typeof(MvcActionLoader).Name);
			list.Add(template.GetField("TagPrefix").ID, "mvc");
			list.Add(template.GetField("Assembly").ID, typeof(MvcActionLoader).Assembly.GetName().Name);
			list.Add(FieldIDs.Icon, Themes.MapTheme("SoftwareV2/16x16/element_selection.png"));
			return list;
		}
	}
}
