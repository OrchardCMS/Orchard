using System;

namespace Orchard.Caching
{
    public class DefaultCacheContextAccessor : ICacheContextAccessor
    {
        [field: ThreadStatic]
        public static IAcquireContext ThreadInstance { get; set; }

        public IAcquireContext Current
        {
            get { return ThreadInstance; }
            set { ThreadInstance = value; }
        }
    }
}