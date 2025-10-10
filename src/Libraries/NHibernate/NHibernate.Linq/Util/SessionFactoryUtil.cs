using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Metadata;

namespace NHibernate.Linq.Util
{
	public static class SessionFactoryUtil
	{
		public static IDictionary<System.Type, string> GetProxyMetaData(this ISessionFactoryImplementor factory, IDictionary<string, IClassMetadata> metaData)
		{
			var dict = new Dictionary<System.Type, string>();
			foreach (var item in metaData)
			{
				if (item.Value.HasProxy)
				{
					var proxyType = factory.GetEntityPersister(item.Key).ConcreteProxyClass;
					if (proxyType != item.Value.MappedClass && !dict.ContainsKey(proxyType))
					{
						dict.Add(proxyType, item.Key);
					}
				}
			}
			return dict;
		}

		public static IDictionary<string, System.Type> GetEntityNameMetaData(this ISessionFactoryImplementor factory)
		{
			var metaData = factory.GetAllClassMetadata();

			var dict = new Dictionary<string, System.Type>();
			foreach (var item in metaData)
			{
				var type = item.Value.MappedClass;

				dict.Add(item.Key, type);
				if (item.Value.HasProxy)
				{
					var proxyType = factory.GetEntityPersister(item.Key).ConcreteProxyClass;
					if (proxyType != type && !dict.ContainsKey(proxyType.FullName))
					{
						dict.Add(proxyType.FullName, proxyType);
					}
				}
			}
			return dict;
		}
	}
}
