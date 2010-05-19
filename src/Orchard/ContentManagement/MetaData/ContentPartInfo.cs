using System;

namespace Orchard.ContentManagement.MetaData
{
    public class ContentPartInfo
    {
        public string PartName { get; set; }
        public Func<ContentPart> Factory { get; set; }
    }
}
