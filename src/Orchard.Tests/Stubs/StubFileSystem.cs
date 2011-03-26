using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Caching;
using Orchard.Services;

namespace Orchard.Tests.Stubs {
    public class StubFileSystem {
        public class Entry {
            public string Name { get; set; }
        }

        public class DirectoryEntry : Entry {
            private readonly IClock _clock;

            public DirectoryEntry(IClock clock) {
                _clock = clock;
                Entries = new List<Entry>();
            }

            public IList<Entry> Entries { get; private set; }

            public IEnumerable<FileEntry> Files {
                get {
                    return Entries.OfType<FileEntry>();
                }
            }

            public IEnumerable<DirectoryEntry> Directories {
                get {
                    return Entries.OfType<DirectoryEntry>();
                }
            }

            public Entry GetEntry(string name) {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException();

                if (name.Contains(Path.DirectorySeparatorChar) || name.Contains(Path.AltDirectorySeparatorChar))
                    throw new ArgumentException();

                return Entries.FirstOrDefault(e => StringComparer.OrdinalIgnoreCase.Equals(e.Name, name));
            }

            public DirectoryEntry CreateDirectory(string name) {
                var entry = GetEntry(name);

                if (entry == null) {
                    entry = new DirectoryEntry(_clock) { Name = name };
                    this.Entries.Add(entry);
                }

                if (!(entry is DirectoryEntry)) {
                    throw new InvalidOperationException(string.Format("Can't create directory \"{0}\": not a directory.", name));
                }

                return (DirectoryEntry)entry;
            }

            public FileEntry CreateFile(string name) {
                var entry = GetEntry(name);

                if (entry == null) {
                    entry = new FileEntry (_clock) { Name = name };
                    this.Entries.Add(entry);
                }

                if (!(entry is FileEntry)) {
                    throw new InvalidOperationException(string.Format("Can't create file \"{0}\": not a file.", name));
                }

                return (FileEntry)entry;
            }
        }

        public class FileEntry : Entry {
            private readonly IClock _clock;

            public FileEntry(IClock clock) {
                _clock = clock;
                LastWriteTimeUtc = _clock.UtcNow;
                Content = new List<byte>();
            }

            public List<byte> Content { get; private set; }
            public DateTime LastWriteTimeUtc { get; set; }
        }

        public class Token : IVolatileToken {
            private readonly StubFileSystem _stubFileSystem;
            private readonly string _path;
            private bool _isCurrent;

            public Token(StubFileSystem stubFileSystem, string path) {
                _stubFileSystem = stubFileSystem;
                _path = path;
                _isCurrent = true;
            }

            public bool IsCurrent { get { return _isCurrent; } }

            public void OnChange() {
                _isCurrent = false;
                _stubFileSystem.DetachToken(_path);
            }
        }

        public class FileEntryWriteStream : Stream {
            private readonly Token _token;
            private readonly FileEntry _entry;
            private readonly IClock _clock;
            private long _position;

            public FileEntryWriteStream(Token token, StubFileSystem.FileEntry entry, IClock clock) {
                _token = token;
                _entry = entry;
                _clock = clock;
            }

            public override void Flush() {
            }

            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotImplementedException();
            }

            public override void SetLength(long value) {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count) {
                throw new NotImplementedException();
            }

            public class ArrayWrapper<T> : ICollection<T> {
                private readonly T[] _buffer;
                private readonly int _offset;
                private readonly int _count;

                public ArrayWrapper(T[] buffer, int offset, int count) {
                    _buffer = buffer;
                    _offset = offset;
                    _count = count;
                }

                public IEnumerator<T> GetEnumerator() {
                    for (int i = _offset; i < _count; i++)
                        yield return _buffer[i];
                }

                IEnumerator IEnumerable.GetEnumerator() {
                    return GetEnumerator();
                }

                public void Add(T item) {
                    throw new NotImplementedException();
                }

                public void Clear() {
                    throw new NotImplementedException();
                }

                public bool Contains(T item) {
                    throw new NotImplementedException();
                }

                public void CopyTo(T[] array, int arrayIndex) {
                    Array.Copy(_buffer, _offset, array, arrayIndex, _count);
                }

                public bool Remove(T item) {
                    throw new NotImplementedException();
                }

                public int Count {
                    get { return _count; }
                }

                public bool IsReadOnly {
                    get { return true; }
                }
            }

            public override void Write(byte[] buffer, int offset, int count) {
                if (count == 0)
                    return;

                var wrapper = new ArrayWrapper<byte>(buffer, offset, count);
                _entry.Content.AddRange(wrapper);

                _entry.LastWriteTimeUtc = _clock.UtcNow;
                if (_token != null)
                    _token.OnChange();
                _position += count;
            }

            public override bool CanRead {
                get { return false; }
            }

            public override bool CanSeek {
                get { return false; }
            }

            public override bool CanWrite {
                get { return true; }
            }

            public override long Length {
                get { return _entry.Content.Count; }
            }

            public override long Position {
                get { return _position; }
                set { throw new NotImplementedException(); }
            }
        }

        public class FileEntryReadStream : Stream {
            private readonly FileEntry _entry;
            private readonly IClock _clock;
            private int _position;

            public FileEntryReadStream(FileEntry entry, IClock clock) {
                _entry = entry;
                _clock = clock;
            }

            public FileEntry FileEntry { get { return _entry; } }

            public override void Flush() {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin) {
                switch (origin) {
                    case SeekOrigin.Begin:
                        _position = (int)offset;
                        break;
                    case SeekOrigin.Current:
                        _position += (int) offset;
                        break;
                    case SeekOrigin.End:
                        _position = _entry.Content.Count - (int) offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("origin");
                }
                return _position;
            }

            public override void SetLength(long value) {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count) {
                int remaingCount = _entry.Content.Count - _position;
                count = Math.Min(count, remaingCount);

                _entry.Content.CopyTo(_position, buffer, offset, count);

                _position += count;
                return count;
            }

            public override void Write(byte[] buffer, int offset, int count) {
                throw new NotImplementedException();
            }

            public override bool CanRead {
                get { return true; }
            }

            public override bool CanSeek {
                get { return true; }
            }

            public override bool CanWrite {
                get { return false; }
            }

            public override long Length {
                get { return _entry.Content.Count; }
            }

            public override long Position {
                get { return _position; }
                set { _position = (int) value; }
            }
        }

        private readonly IClock _clock;
        private readonly DirectoryEntry _root;
        private readonly Dictionary<string, Weak<Token>> _tokens;

        public StubFileSystem(IClock clock) {
            _clock = clock;
            _root = new DirectoryEntry(_clock);
            _tokens = new Dictionary<string, Weak<Token>>(StringComparer.OrdinalIgnoreCase);
        }

        public DirectoryEntry GetDirectoryEntry(string path) {
            // Root is a special case: it has no name.
            if (string.IsNullOrEmpty(path))
                return _root;

            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var current = _root;
            foreach (var name in path.Split(Path.DirectorySeparatorChar)) {
                current = current.GetEntry(name) as DirectoryEntry;
                if (current == null)
                    break;
            }
            return current;
        }

        public DirectoryEntry CreateDirectoryEntry(string path) {
            // Root is a special case: it has no name.
            if (string.IsNullOrEmpty(path))
                return _root;

            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var current = _root;
            foreach (var name in path.Split(Path.DirectorySeparatorChar)) {
                current = current.CreateDirectory(name);
            }
            return current;
        }

        public FileEntry GetFileEntry(string path) {
            var directoryName = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var directory = GetDirectoryEntry(directoryName);
            if (directory == null)
                return null;

            return directory.GetEntry(fileName) as StubFileSystem.FileEntry;
        }

        public FileEntry CreateFileEntry(string path) {
            var directoryName = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            return CreateDirectoryEntry(directoryName).CreateFile(fileName);
        }

        public IVolatileToken WhenPathChanges(string path) {
            Token token = GetToken(path);

            if (token == null) {
                token = new Token(this, path);
                _tokens.Add(path, new Weak<Token>(token));
            }
            return token;
        }

        private Token GetToken(string path) {
            Token token = null;
            Weak<Token> weakRef;
            if (_tokens.TryGetValue(path, out weakRef))
                token = weakRef.Target;
            return token;
        }

        private void DetachToken(string path) {
            _tokens.Remove(path);
        }

        public Stream CreateFile(string path) {
            var entry = CreateFileEntry(path);
            entry.Content.Clear();
            return new FileEntryWriteStream(GetToken(path), entry, _clock);
        }

        public Stream OpenFile(string path) {
            var entry = GetFileEntry(path);
            if (entry == null)
                throw new InvalidOperationException();

            return new FileEntryReadStream(entry, _clock);
        }

        public void DeleteFile(string path) {
            var directoryName = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var directory = GetDirectoryEntry(directoryName);
            if (directory == null)
                return;

            var entry = directory.GetEntry(fileName);
            if (entry == null)
                return;

            directory.Entries.Remove(entry);
        }
    }
}