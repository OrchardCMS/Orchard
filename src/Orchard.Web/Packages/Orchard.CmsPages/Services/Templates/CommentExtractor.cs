using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orchard.CmsPages.Services.Templates {
    /// <summary>
    /// Parse the content of a text reader into a list of metadata entries.
    /// </summary>
    public class CommentExtractor {
        private readonly Regex _commentRegex;

        public CommentExtractor() {
            _commentRegex = new Regex(@"<%--(?<comment>(.|\s)*?)--%>", RegexOptions.Multiline);
        }

        public IList<string> Process(TextReader reader) {
            //TODO: Performance: Reading the whole file into a string is not GC friendly
            //      especially if the file is big (the string will go to the large object table).
            //      We could update the implementation to use a very simple parser instead of using
            //      regex here.
            var result = new List<string>();

            MatchCollection matches = _commentRegex.Matches(reader.ReadToEnd());
            foreach (Match match in matches) {
                result.Add(match.Groups["comment"].Value);
            }

            return result;
        }

        public TextReader FirstComment(TextReader reader) {
            var result = Process(reader).FirstOrDefault();
            return new StringReader(result ?? string.Empty);
        }
    }
}