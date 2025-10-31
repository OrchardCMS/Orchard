using System.Collections;

namespace NHibernate.Linq.Transform
{
	public class LinqJoinResultsTransformer : NHibernate.Transform.IResultTransformer
	{
		private readonly System.Type _entityType;

		public LinqJoinResultsTransformer(System.Type entityType)
		{
			_entityType = entityType;
		}

		public IList TransformList(IList collection)
		{
			return collection;
		}

		public object TransformTuple(object[] tuple, string[] aliases)
		{
			foreach (object obj in tuple)
			{
				if (obj != null && obj.GetType() == _entityType)
				{
					return obj;
				}
			}

			return null;
		}
	}
}
