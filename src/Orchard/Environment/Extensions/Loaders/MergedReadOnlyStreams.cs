using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Expose a read-only stream as the concatenation of a list of read-only streams
    /// </summary>
    public class MergedReadOnlyStreams : Stream {
        private class StreamDescriptor {
            public int Index { get; set; }
            public Stream Stream { get; set; }
            public long Offset { get; set; }
            public long Length { get; set; }
            public long Limit { get { return Offset + Length; } }
        }

        private readonly List<StreamDescriptor> _streams;
        private long _position;

        public MergedReadOnlyStreams(params Stream[] streams) {
            _streams = CreateDescritors(streams).ToList();
        }

        private static IEnumerable<StreamDescriptor> CreateDescritors(params Stream[] streams) {
            long offset = 0;
            int index = 0;
            foreach (var stream in streams) {
                yield return new StreamDescriptor {
                    Stream = stream,
                    Index = index,
                    Length = stream.Length,
                    Offset = offset
                };

                offset += stream.Length;
                index++;
            }
        }
        public override void Flush() {
        }

        public override long Seek(long offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin");
            }
            return _position;
        }

        public override void SetLength(long value) {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int totalRead = 0;
            while (count > 0) {
                // Find stream for current position (might fail if end of all streams)
                var descriptor = GetDescriptor(_position);
                if (descriptor == null)
                    break;

                // Read bytes from the current stream
                int read = descriptor.Stream.Read(buffer, offset, count);
                if (read == 0)
                    break;

                _position += read;
                totalRead += read;
                count -= read;
                offset += read;
            }
            return totalRead;
        }

        private StreamDescriptor GetDescriptor(long position) {
            return _streams.SingleOrDefault(stream => stream.Offset <= position && position < stream.Limit);
        }

        public override void Write(byte[] buffer, int offset, int count) {
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return _streams.All(d => d.Stream.CanSeek); }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override long Length {
            get { return _streams.Aggregate(0L, (prev, desc) => prev + desc.Length); }
        }

        public override long Position {
            get { return _position; }
            set {
                if (!CanSeek)
                    throw new InvalidOperationException();

                _position = Position;

                // Update the position of all streams
                foreach(var desc in _streams) {
                    if (_position < desc.Offset) desc.Stream.Position = 0;
                    else if (_position > desc.Limit) desc.Stream.Position = desc.Stream.Length;
                    else desc.Stream.Position = _position - desc.Offset;
                }
            }
        }
    }
}