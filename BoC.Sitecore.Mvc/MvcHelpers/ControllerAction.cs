using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using BoC.Sitecore.Mvc.Extensions;
using Sitecore.Data;

namespace BoC.Sitecore.Mvc.MvcHelpers
{
	public class ControllerAction: IComparable<ControllerAction>
	{
		public ControllerType ControllerType { get; private set; }
		public string ActionName { get; private set; }
		public Guid ParentId { get; private set; }
		private MethodInfo methodInfo { get; set; }
		private string description;
		public string Description
		{
			get
			{
				if (description == null)
				{
					var attributes = methodInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
					if (attributes != null && attributes.Length > 0)
					{
						description = ((DescriptionAttribute)attributes[0]).Description;
						if (description.Contains("{0}"))
							description = String.Format(description, ActionName);
						else
						{
							description = String.Format("{0} - {1}", ActionName, description);
						}
					}
					if (String.IsNullOrEmpty(description))
						description = ActionName;
				}
				return description;
			}
		}

		private Guid? id;
		public Guid Id
		{
			get
			{
				return id ?? (id = GetId(ParentId, GetKey())).Value;
			}
		}

		internal string GetKey()
		{
			return GetKey(ControllerType.Type, ActionName);
		}

		public int CompareTo(ControllerAction other)
		{
			if (other == null)
				return -1;

			return String.CompareOrdinal(ActionName, other.ActionName);
		}

		private static Guid GetId(Guid parentId, string key)
		{
            //removed IDTable dependency, since that's really for import-like data.. it doesn't support packaging for example
		    return (parentId.ToString().ToLower() + key.ToLower()).ToUniqueGuid();
		}

		private class InternalControllerAction : IComparable<InternalControllerAction>, IComparable
		{
			internal ControllerType ControllerType { get; set; }
			internal string ActionName { get; set; }
			internal MethodInfo MethodInfo { get; set; }
			public int CompareTo(InternalControllerAction other)
			{
				if (other == null)
					return -1;

				return String.CompareOrdinal(GetKey(ControllerType.Type, ActionName), GetKey(other.ControllerType.Type, other.ActionName));
			}

			int IComparable.CompareTo(object obj)
			{
				return this.CompareTo(obj as InternalControllerAction);
			}
		}
		#region static helpers
		internal static string GetKey(Type controllerType, string actionName)
		{
			return string.Format("{0}/{1}", controllerType.FullName, actionName);
		}

		public static ControllerAction GetControllerAction(ID parentId, ID itemId)
		{
			return GetAllActions(parentId.ToGuid()).FirstOrDefault(action => action.Id == itemId.ToGuid());
		}

		private static object idlock = new object();
		private static IDictionary<Guid, ISet<ControllerAction>> allActionIds = new Dictionary<Guid, ISet<ControllerAction>>();
		public static ISet<ControllerAction> GetAllActions(Guid parentId)
		{
			if (!allActionIds.ContainsKey(parentId))
			{
				lock (idlock)
				{
					if (!allActionIds.ContainsKey(parentId))
					{
						allActionIds.Add(parentId, new HashSet<ControllerAction>(
							GetAllActions().Select(action => new ControllerAction()
										{
											ControllerType = action.ControllerType,
											ParentId = parentId,
											ActionName = action.ActionName,
											methodInfo = action.MethodInfo
										})));
					}
				}
			}
			return allActionIds[parentId];
		}

		private static object keylock = new object();
		private static ISet<InternalControllerAction> allActions = new SortedSet<InternalControllerAction>();
		private static IEnumerable<InternalControllerAction> GetAllActions()
		{
			if (allActions.Count() == 0)
			{
				lock (keylock)
				{
					if (allActions.Count()==0)
					{
						var controllers = MvcHelpers.ControllerType.GetAllControllers();
						foreach (var controllerType in controllers)
						{
							MethodInfo[] allMethods = controllerType.Type.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
							MethodInfo[] actionMethods = Array.FindAll(allMethods, IsValidActionMethod);

							var aliasedMethods = Array.FindAll(actionMethods, info => info.IsDefined(typeof(ActionNameAttribute), true /* inherit */));
							var nonAliasedMethods = actionMethods.Except(aliasedMethods);
							foreach (var aliasedMethod in aliasedMethods)
							{
								var name = aliasedMethod.GetCustomAttributes(typeof(ActionNameAttribute), true).OfType<ActionNameAttribute>().FirstOrDefault();
								if (name != null)
								{
									allActions.Add(new InternalControllerAction
									               	{
									               		ActionName = name.Name,
									               		ControllerType = controllerType,
														MethodInfo = aliasedMethod
									               	});
								}
							}
							foreach (var method in nonAliasedMethods)
							{
								allActions.Add(new InternalControllerAction
								{
									ActionName = method.Name,
									ControllerType = controllerType,
									MethodInfo = method
								});
							}
						}
					}
				}
			}
			return allActions;
		}

		static bool IsValidActionMethod(MethodInfo methodInfo)
		{
			return !(methodInfo.IsSpecialName ||
					 methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(Controller)));
		}
		#endregion
	}
}
