using System;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.Utilities;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;

namespace Orchard.Core.Common.Models {
    public class CommonAspect : ContentPart<CommonRecord>, ICommonAspect {
        private readonly LazyField<IUser> _owner = new LazyField<IUser>();
        private readonly LazyField<IContent> _container = new LazyField<IContent>();

        public LazyField<IUser> OwnerField {
            get { return _owner; }
        }
        
        public LazyField<IContent> ContainerField {
            get { return _container; }
        }


        DateTime? ICommonAspect.CreatedUtc {
            get { return Record.CreatedUtc;}
            set { Record.CreatedUtc = value;}
        }

        DateTime? ICommonAspect.ModifiedUtc {
            get { return Record.ModifiedUtc;}
            set { Record.ModifiedUtc = value;}
        }

        public IUser Owner {
            get { return _owner.Value; }
            set { _owner.Value = value; }
        }

        public IContent Container {
            get { return _container.Value; }
            set { _container.Value = value; }
        }
    }
}
