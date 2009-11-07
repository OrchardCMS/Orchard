using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orchard.CmsPages.Services.Templates {
    /// <summary>
    /// Parse the content of a text reader into a list of metadata entries.
    /// </summary>
    public class MetadataParser {
        private readonly Regex _tagRegex;

        public MetadataParser() {
            _tagRegex = new Regex(@"^\s*(?<tag>\w+)\s*:\s*(?<value>)", RegexOptions.Multiline);
        }

        public IList<MetadataEntry> Parse(TextReader reader) {
            string content = reader.ReadToEnd();
            var result = new List<MetadataEntry>();

            // Find matches
            MatchCollection matches = _tagRegex.Matches(content);

            // Process each match
            for (int i = 0; i < matches.Count; i++) {
                Match currentMatch = matches[i];
                Match nextMatch = (i < matches.Count - 1 ? matches[i + 1] : null);

                //int tagIndex = currentMatch.Groups["tag"].Index;
                string tag = currentMatch.Groups["tag"].Value;

                int valueIndex = currentMatch.Groups["value"].Index;
                string value =
                    nextMatch == null ?
                                 content.Substring(valueIndex) :
                                 content.Substring(valueIndex, nextMatch.Groups["tag"].Index - valueIndex);

                // Remove optional trailing space and line separators at end of value
                int count = 0;
                foreach (char ch in value.Reverse()) {
                    if (char.IsSeparator(ch) || char.IsWhiteSpace(ch) || ch == '\r' || ch == '\n') {
                        count++;
                    }
                    else {
                        break;
                    }
                }
                value = value.Substring(0, value.Length - count);

                // Add result entry
                result.Add(new MetadataEntry { Tag = tag, Value = value });
            }
            return result;
        }
    }
}