using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orchard.ResponseFiles {
    public class ResponseLine : MarshalByRefObject {
        public string Filename { get; set; }
        public string LineText { get; set; }
        public int LineNumber { get; set; }
        public string[] Args { get; set; }
    }

    public class ResponseFileReader {
        public IEnumerable<ResponseLine> ReadLines(string filename) {
            using (var reader = File.OpenText(filename)) {
                for (int i = 0; ; i++) {
                    string lineText = reader.ReadLine();
                    if (lineText == null)
                        yield break;

                    yield return new ResponseLine {
                        Filename = filename,
                        LineText = lineText,
                        LineNumber = i,
                        Args = SplitArgs(lineText).ToArray()
                    };
                }
            }
        }

        public static IEnumerable<string> SplitArgs(string text) {
            var sb = new StringBuilder();
            bool inString = false;
            foreach (char ch in text) {
                switch(ch){
                    case '"':
                        if (inString) {
                            inString = false;
                            yield return sb.ToString();
                            sb.Length = 0;
                        }
                        else {
                            inString = true;
                            sb.Length = 0;
                        }
                        break;

                    case ' ':
                    case '\t':
                        if (sb.Length > 0) {
                            yield return sb.ToString();
                            sb.Length = 0;
                        }
                        break;

                    default:
                        sb.Append(ch);
                        break;
                }
            }

            // If there was anything accumulated
            if (sb.Length > 0) {
                yield return sb.ToString();
                sb.Length = 0;
            }
        }
    }
}
