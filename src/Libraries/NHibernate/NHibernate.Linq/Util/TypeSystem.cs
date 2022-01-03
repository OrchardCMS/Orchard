using System.Collections.Generic;

namespace NHibernate.Linq.Util
{
	/// <remarks>
	/// http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
	/// </remarks>
	public static class TypeSystem
	{
		public static System.Type GetElementType(System.Type seqType)
		{
			System.Type ienum = FindIEnumerable(seqType);
			if (ienum == null) return seqType;
			return ienum.GetGenericArguments()[0];
		}

		private static System.Type FindIEnumerable(System.Type seqType)
		{
			if (seqType == null || seqType == typeof(string))
				return null;

			if (seqType.IsArray)
				return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

			if (seqType.IsGenericType)
			{
				foreach (System.Type arg in seqType.GetGenericArguments())
				{
					System.Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
					if (ienum.IsAssignableFrom(seqType))
					{
						return ienum;
					}
				}
			}

			System.Type[] ifaces = seqType.GetInterfaces();
			if (ifaces != null && ifaces.Length > 0)
			{
				foreach (System.Type iface in ifaces)
				{
					System.Type ienum = FindIEnumerable(iface);
					if (ienum != null) return ienum;
				}
			}

			if (seqType.BaseType != null && seqType.BaseType != typeof(object))
			{
				return FindIEnumerable(seqType.BaseType);
			}
			return null;
		}
	}
}
