using System.Collections.Generic;

namespace Orchard.Arguments {
    public interface IParser {
        ParserResult Parse(IEnumerable<string> args);
    }
}