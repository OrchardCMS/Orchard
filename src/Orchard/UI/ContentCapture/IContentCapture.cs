using System.Collections.Generic;
using System.IO;

namespace Orchard.UI.ContentCapture {
    public interface IContentCapture : IDependency {
        Stream CaptureStream { get; set; }
        Dictionary<string, string> GetContents();
        void BeginContentCapture(string name);
        void EndContentCapture();
    }
}