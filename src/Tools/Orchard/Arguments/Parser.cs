using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Arguments {
    public class Parser : IParser {
        public ParserResult Parse(IEnumerable<string> args) {
            ParserResult result = new ParserResult();

            IEnumerator<string> e = args.GetEnumerator();
            while (e.MoveNext()) {
                if (e.Current[0] == '/') {
                    var s = ParseSwitch(e);
                    result.Switches.Add(s);
                }
                else {
                    result.Arguments.Add(e.Current);
                }
            }

            return result;
        }

        private Switch ParseSwitch(IEnumerator<string> enumerator) {
            string sw = enumerator.Current.Substring(1);
            string[] args = sw.Split(':');
            return new Switch {
                                  Name = args[0],
                                  Value = args.Length >= 2 ? args[1] : string.Empty
                              };
        }
    }
}