using System;
using System.Runtime.Serialization;

namespace Orchard.Caching {
    public class Weak<T> : WeakReference {
        public Weak(T target)
            : base(target) {
        }

        public Weak(T target, bool trackResurrection)
            : base(target, trackResurrection) {
        }

        protected Weak(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public new T Target {
            get { return (T)base.Target; }
            set { base.Target = value; }
        }
    }
}
