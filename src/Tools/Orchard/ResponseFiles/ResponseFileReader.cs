using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Parameters;

namespace Orchard.ResponseFiles {
    public class ResponseLine : MarshalByRefObject {
        public string Filename { get; set; }
        public string LineText { get; set; }
        public int LineNumber { get; set; }
        public string[] Args { get; set; }

        public override object InitializeLifetimeService() {
            // never expire the cross-AppDomain lease on this object
            return null;
        }
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
                        Args = new CommandLineParser().Parse(lineText).ToArray()
                    };
                }
            }
        }
    }
}
