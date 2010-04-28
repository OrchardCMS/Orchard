using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orchard.ResponseFiles {
    public class ResponseFiles {
        public IEnumerable<ResponseLine> ReadFiles(IEnumerable<string> filenames) {
            foreach (var filename in filenames) {
                foreach(var line in new ResponseFileReader().ReadLines(filename))
                {
                    yield return line;
                }
            }
        }
    }
}
