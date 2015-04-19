using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Orchard.OutputCache.Filters {
    public class CaptureStream : Stream {
        public CaptureStream(Stream innerStream) {
            _innerStream = innerStream;
            _captureStream = new MemoryStream();
        }

        private readonly Stream _innerStream;
        private readonly MemoryStream _captureStream;

        public override bool CanRead {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return _innerStream.CanWrite; }
        }

        public override long Length {
            get { return _innerStream.Length; }
        }

        public override long Position {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        public override long Seek(long offset, SeekOrigin direction) {
            return _innerStream.Seek(offset, direction);
        }

        public override void SetLength(long length) {
            _innerStream.SetLength(length);
        }

        public override void Close() {
            _innerStream.Close();
        }

        public override void Flush() {
            if (_captureStream.Length > 0) {
                OnCaptured();
                _captureStream.SetLength(0);
            }

            _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _captureStream.Write(buffer, offset, count);
            _innerStream.Write(buffer, offset, count);
        }

        public event Action<byte[]> Captured;

        protected virtual void OnCaptured() {
            Captured(_captureStream.ToArray());
        }
    }
}