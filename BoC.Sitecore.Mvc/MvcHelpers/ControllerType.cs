using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using BoC.Sitecore.Mvc.Extensions;
using Sitecore.Data;
using Sitecore.Data.IDTables;

namespace BoC.Sitecore.Mvc.MvcHelpers
{
	public class ControllerType
	{
		private ControllerType(Type type)
		{
			this.Type = type;
		}

		public Type Type { get; private set; }

		private string description;
		public string Description
		{
			get
			{
				if (description == null)
				{
					var attributes = Type.GetCustomAttributes(typeof (DescriptionAttribute), false);
					if (attributes != null && attributes.Length > 0)
					{
						description = ((DescriptionAttribute) attributes[0]).Description;
						if (description.Contains("{0}"))
							description = String.Format(description, ControllerName);
						else
						{
							description = String.Format("{0} - {1}", ControllerName, description);
						}
					}
					if (String.IsNullOrEmpty(description))
						description = ControllerName;
				}
				return description;
			}
		}

		public string ControllerName
		{
			get
			{
				if (Type.Name.EndsWith("Controller", StringComparison.CurrentCultureIgnoreCase))
				{
					return Type.Name.Substring(0, Type.Name.Length - "controller".Length);
				}
				return Type.Name;
			}
		}

		#region static helpers
		private static Guid GetId(Guid parentId, string key)
		{
            //removed IDTable dependency, since that's really for import-like data.. it doesn't support packaging for example
		    return (parentId.ToString().ToLower() + key.ToLower()).ToUniqueGuid();
		}

		public static ControllerType GetControllerType(ID parentId, ID id)
		{
			ControllerType type;
			if (GetControllerIds(parentId.ToGuid()).TryGetValue(id.ToGuid(), out type))
				return type;
			return null;
		}

		private static object controllerId_lock = new object();
		static IDictionary<Guid, IDictionary<Guid, ControllerType>> controllerIds = new Dictionary<Guid, IDictionary<Guid, ControllerType>>();
		public static IDictionary<Guid, ControllerType> GetControllerIds(Guid parentId)
		{
			if (!controllerIds.ContainsKey(parentId))
			{
				lock (controllerId_lock)
				{
					if (!controllerIds.ContainsKey(parentId))
					{
						controllerIds.Add(parentId, GetAllControllers().ToDictionary(t => GetId(parentId, t.Type.AssemblyQualifiedName), t => t));
					}
				}
			}
			return controllerIds[parentId];
		}

		private static IEnumerable<ControllerType> allControllers;

		internal static IEnumerable<ControllerType> GetAllControllers()
		{
			return allControllers ?? (allControllers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
				{
					try
					{
						return a.GetTypes().Where(t => 
							!t.IsAbstract &&
							!t.IsInterface &&
							t.IsClass &&
							typeof(IController).IsAssignableFrom(t)).ToList();
					}
					catch { return Enumerable.Empty<Type>(); }
				}).Select(t => new ControllerType(t)));
		}
		#endregion
	}
}
