using System.Collections.Generic;

namespace Orchard.Tags.Helpers
{
    public class TagHelpers
    {
        public static List<string> ParseCommaSeparatedTagNames(string tags)
        {
            if (string.IsNullOrEmpty(tags))
            {
                return new List<string>();
            }
            IEnumerable<string> tagNames = tags.Split(',');
            var sanitizedTagNames = new List<string>();
            foreach (var tagName in tagNames)
            {
                string sanitizedTagName = tagName.Trim();
                if (!string.IsNullOrEmpty(sanitizedTagName))
                {
                    sanitizedTagNames.Add(sanitizedTagName);
                }
            }
            return sanitizedTagNames;
        }
    }
}