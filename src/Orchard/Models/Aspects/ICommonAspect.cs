using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Security;

namespace Orchard.ContentManagement.Aspects {
    public interface ICommonAspect : IContent {
        DateTime? CreatedUtc { get; set; }
        DateTime? ModifiedUtc { get; set; }
        IUser Owner { get; set; }
        IContent Container { get; set; }
    }
}
