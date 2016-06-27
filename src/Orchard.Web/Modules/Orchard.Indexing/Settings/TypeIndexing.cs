using System;

namespace Orchard.Indexing.Settings {
    public class TypeIndexing {
        public string Indexes { get; set; }
        
        public string[] List {
            get { return String.IsNullOrEmpty(Indexes) ? new string[0] : Indexes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
            set { Indexes = String.Join(",", value); }
        }
    }
}
