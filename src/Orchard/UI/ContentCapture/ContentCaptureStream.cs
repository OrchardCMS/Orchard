using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orchard.UI.ContentCapture {
    public class ContentCaptureStream : Stream, IContentCapture {
        private List<byte> _contentBuffer;
        private string _contentCaptureName;
        private readonly Dictionary<string, string> _contents;

        public ContentCaptureStream() {
            _contents = new Dictionary<string, string>(20);
        }

        public Stream CaptureStream { get; set; }

        public override bool CanRead {
            get { return CaptureStream.CanRead; }
        }

        public override bool CanSeek {
            get { return CaptureStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return CaptureStream.CanWrite; }
        }

        public override long Length {
            get { return CaptureStream.Length; }
        }

        public override long Position {
            get { return CaptureStream.Position; }
            set { CaptureStream.Position = value; }
        }

        public override void Flush() {
            CaptureStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return CaptureStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return CaptureStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            CaptureStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (_contentCaptureName != null)
                _contentBuffer.AddRange(buffer);
            else
                CaptureStream.Write(buffer, 0, buffer.Length);
        }

        public Dictionary<string, string> GetContents() {
            if (_contentCaptureName != null)
                throw new ApplicationException("Can not get contents while capturing content");

            Dictionary<string, string> contents = new Dictionary<string, string>(_contents.Count);

            foreach (KeyValuePair<string, string> content in contents)
                contents.Add(content.Key, content.Value);
            _contents.Clear();

            return contents;
        }

        public void BeginContentCapture(string name) {
            if (_contentCaptureName != null)
                throw new ApplicationException("There is already content being captured");

            _contentBuffer = new List<byte>(5000);
            _contentCaptureName = name;
        }

        public void EndContentCapture() {
            if (_contentCaptureName == null)
                throw new ApplicationException("There is currently no content being captured");

            _contents.Add(_contentCaptureName, Encoding.UTF8.GetString(_contentBuffer.ToArray(), 0, _contentBuffer.Count));
            _contentCaptureName = null;
            _contentBuffer = null;
        }
    }
}