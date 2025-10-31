using System;
using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Tags.Models
{
    public class TagsPart : ContentPart<TagsPartRecord>
    {
        public IEnumerable<string> CurrentTags
        {
            get { return ParseTags(Retrieve<string>("CurrentTags")); }
            set { Store("CurrentTags", string.Join(",", value)); }
        }

        private IEnumerable<string> ParseTags(string tags)
        {
            return (tags ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}