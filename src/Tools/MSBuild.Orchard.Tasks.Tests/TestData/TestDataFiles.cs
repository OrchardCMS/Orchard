using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuild.Orchard.Tasks.Tests.TestData {
    public class TestDataFiles : IDisposable {
        List<Entry> _entries = new List<Entry>();
        private readonly string _tempPath;

        public TestDataFiles() {
            _tempPath = Path.GetTempFileName();
            File.Delete(_tempPath);
            Directory.CreateDirectory(_tempPath);
        }

        public void Dispose() {
            Directory.Delete(_tempPath, true);
        }

        public string Get(string name) {
            if (!_entries.Any(entry => entry.Name == name)) {
                var type = GetType();
                var fullPath = Path.Combine(_tempPath, name);
                using (var inputStream = type.Assembly.GetManifestResourceStream(type, name)) {
                    if (inputStream == null)
                        throw new ApplicationException("Tests data not found");

                    using (var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                        var buffer = new byte[8192];
                        for (; ; ) {
                            var size = inputStream.Read(buffer, 0, buffer.Length);
                            if (size < 1)
                                break;
                            outputStream.Write(buffer, 0, size);
                        }
                    }
                }
                _entries.Add(new Entry { Name = name, FullPath = fullPath });
            }
            return _entries.Single(entry => entry.Name == name).FullPath;
        }

        class Entry {
            public string Name { get; set; }
            public string FullPath { get; set; }
        }
    }
}
