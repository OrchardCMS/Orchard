using System;

namespace Orchard.ContentManagement.MetaData
{
    public class ContentPartInfo
    {
        public string partName;
        public Func<ContentPart> Factory;
    }
}
