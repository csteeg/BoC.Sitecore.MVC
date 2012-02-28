using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using BoC.Sitecore.Mvc.MvcHelpers;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.DataProviders;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;

namespace BoC.Sitecore.Mvc
{
	public class MvcLayoutDataProvider : DataProvider
	{
		private readonly ID masterId = ID.Null;
		protected virtual ID MasterId { get { return masterId; } }
		protected virtual ID BaseTemplateId { get { return TemplateIDs.Layout; } }
	    internal static ID parentID = ItemIDs.Layouts;
        protected virtual ID ParentId { get { return parentID; } }
		private readonly ID folderTemplateId = new ID("{93227C5D-4FEF-474D-94C0-F252EC8E8219}"); ///sitecore/templates/System/Layout/Layout Folder
		protected virtual ID FolderTemplateId { get { return folderTemplateId; } }
		private readonly ID folderId = new ID("{11111111-1EE3-4181-A7E8-DFC489EAB2C4}");
		protected virtual ID FolderId { get { return folderId; } }
		protected virtual string FolderName { get { return "MVC Actions"; } }
		
		// :( not possible to add own layout-templates
		//private readonly ID templateFolderId = new ID("{4E1FFE6E-368D-4851-A974-BC101C9371B5}");
		//protected virtual ID TemplateFolderId { get { return templateFolderId; } }
		//private readonly ID temlateId = new ID("{11111111-1EE3-4181-A7E8-DFC489EAB2C5}");
		//protected virtual ID TemplateId { get { return temlateId; } }
		//private readonly ID field_ControllerId = new ID("{11111111-1EE3-4181-A7E8-DFC489EAB2C6}");
		//protected virtual ID Field_ControllerId { get { return field_ControllerId; } }
		//private readonly ID field_ActionId = new ID("{11111111-1EE3-4181-A7E8-DFC489EAB2C7}");
		//protected virtual ID Field_ActionId { get { return field_ActionId; } }
		//private readonly ID sectionId = new ID("{11111111-1EE3-4181-A7E8-DFC489EAB2C8}");
		//protected virtual ID SectionId { get { return sectionId; } }
		//private readonly string templateName = "Mvc Layout Template";
		//protected virtual string TemplateName { get { return templateName; } }

		public override ItemDefinition GetItemDefinition(ID itemId, CallContext context)
		{
			Assert.ArgumentNotNull(itemId, "itemId");
			Assert.ArgumentNotNull(context, "context");
			if (itemId == FolderId)
				return new ItemDefinition(itemId, FolderName, FolderTemplateId, this.MasterId);

			var type = ControllerType.GetControllerType(ParentId, itemId);
			if (type != null)
			{
				return new ItemDefinition(itemId, ItemUtil.ProposeValidItemName(type.Description), FolderTemplateId, this.MasterId);
			}

			var action = ControllerAction.GetControllerAction(ParentId, itemId);
			if (action != null)
			{
				return new ItemDefinition(itemId, action.Description, BaseTemplateId, this.MasterId);
			}

			return base.GetItemDefinition(itemId, context);
		}

		public override ID GetParentID(ItemDefinition itemDefinition, CallContext context)
		{
			if (itemDefinition.ID == FolderId)
				return ParentId;

			if (ControllerType.GetControllerType(ParentId, itemDefinition.ID) != null)
			{
				return FolderId;
			}

			ControllerAction action;
			if ((action = ControllerAction.GetControllerAction(ParentId, itemDefinition.ID)) != null)
			{
				return ControllerType.GetControllerIds(ParentId.ToGuid()).Where(kv => kv.Value.Type == action.ControllerType.Type)
					.Select(kv => new ID(kv.Key)).FirstOrDefault();
			}
			
			//if (itemDefinition.ID == TemplateFolderId)
			//    return ItemIDs.TemplateRoot;
			//if (itemDefinition.ID == TemplateId)
			//    return TemplateFolderId;
			return base.GetParentID(itemDefinition, context);
		}

		public override FieldList GetItemFields(ItemDefinition item, VersionUri version, CallContext context)
		{
			Assert.ArgumentNotNull(item, "item");
			Assert.ArgumentNotNull(version, "version");
			Assert.ArgumentNotNull(context, "context");

			var list = new FieldList();
			if (item.ID == FolderId || (ControllerType.GetControllerType(ParentId, item.ID) != null))
			{
				list.Add(FieldIDs.Icon, Themes.MapTheme("SoftwareV2/16x16/elements.png"));
			}
			else
			{
				var action = ControllerAction.GetControllerAction(ParentId, item.ID);
				if (action != null && HttpContext.Current != null)
				{
					VirtualPathData vpd;
					MvcActionHelper.GetRouteData(new HttpContextWrapper(HttpContext.Current), action.ActionName, action.ControllerType.ControllerName, null, false, out vpd);

					list.Add(LayoutFieldIDs.Path, vpd.VirtualPath);
					list.Add(FieldIDs.Icon, Themes.MapTheme("SoftwareV2/16x16/element.png"));
				}
			}
			if (list.Count == 0)
				return base.GetItemFields(item, version, context);

			return list;
		}


		public override IDList GetChildIDs(ItemDefinition itemDefinition, CallContext context)
		{
			ControllerType controllerType;

			var list = base.GetChildIDs(itemDefinition, context) ?? new IDList();
			if (itemDefinition.ID == ParentId)
				list.Add(FolderId);
			else if (itemDefinition.ID == FolderId)
			{
				AddAllControllers(list);
			}
			else if ((controllerType = ControllerType.GetControllerType(ParentId, itemDefinition.ID)) != null)
			{
				AddAllActions(list, controllerType);
			}
			//else if ((itemDefinition.ID == ItemIDs.TemplateRoot)  && (this.GetType() == typeof(MvcLayoutDataProvider)))
			//    list.Add(TemplateFolderId);
			//else if (itemDefinition.ID == TemplateFolderId)
			//    list.Add(this.TemplateId);
			return list;
		}

		void AddAllActions(IDList list, ControllerType controllerType)
		{
			foreach (var action in ControllerAction.GetAllActions(ParentId.ToGuid()).Where(a => a.ControllerType.Type == controllerType.Type))
			{
				list.Add(new ID(action.Id));
			}
		}

		void AddAllControllers(IDList list)
		{
			foreach (var controller in ControllerType.GetControllerIds(ParentId.ToGuid()))
			{
				list.Add(new ID(controller.Key));
			}
		}
	}
}
