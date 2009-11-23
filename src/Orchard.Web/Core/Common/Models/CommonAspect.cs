using System;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Models;
using Orchard.Security;

namespace Orchard.Core.Common.Models {
    public class CommonAspect : ContentPart<CommonRecord> {
        private readonly LazyField<IUser> _owner = new LazyField<IUser>();
        private readonly LazyField<IContent> _container = new LazyField<IContent>();

        public IUser Owner {
            get { return _owner.Value; }
            set {_owner.Value = value;}
        }

        public IContent Container {
            get { return _container.Value; }
            set {_container.Value = value;}
        }

        internal void OnGetOwner(Func<IUser> loader) {
            _owner.Loader(loader);
        }
        internal void OnSetOwner(Func<IUser,IUser> setter) {
            _owner.Setter(setter);
        }


        internal void OnGetContainer(Func<IContent> loader) {
            _container.Loader(loader);
        }
        internal void OnSetContainer(Func<IContent, IContent> setter) {
            _container.Setter(setter);
        }
    }
}
