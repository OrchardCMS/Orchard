using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Orchard.FileSystems.Media;
using Orchard.Azure.MediaServices.Helpers;
using Newtonsoft.Json;

namespace Orchard.Azure.MediaServices.Models.Assets.EncoderMetadata {
    public class AssetFile {

        private readonly XmlNamespaceManager _nsm;
        private readonly XElement _xml;
        private readonly Metadata _parentMetadata;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private IEnumerable<AudioTrack> _audioTracks;
        private IEnumerable<VideoTrack> _videoTracks;
        private IEnumerable<string> _sources;

        public AssetFile(XElement xml, Metadata parentMetadata, IMimeTypeProvider mimeTypeProvider) {
            _nsm = NamespaceHelper.CreateNamespaceManager(xml);
            _xml = xml;
            _parentMetadata = parentMetadata;
            _mimeTypeProvider = mimeTypeProvider;
        }

        /// <summary>
        /// The name of the media file.
        /// </summary>
        public string Name {
            get {
                return _xml.Attribute(XName.Get("Name")).Value;
            }
        }

        /// <summary>
        /// The size of the media file in bytes.
        /// </summary>
        public long Size {
            get {
                return XmlConvert.ToInt64(_xml.Attribute(XName.Get("Size")).Value);
            }
        }

        /// <summary>
        /// The play back duration of the media file.
        /// </summary>
        public TimeSpan Duration {
            get {
                return XmlConvert.ToTimeSpan(_xml.Attribute(XName.Get("Duration")).Value);
            }
        }

        /// <summary>
        /// A collection of audio tracks contained in the media file.
        /// </summary>
        public IEnumerable<AudioTrack> AudioTracks {
            get {
                if (_audioTracks == null) {
                    var audioTracksQuery =
                        from e in _xml.XPathSelectElements("./me:AudioTracks/me:AudioTrack", _nsm)
                        select new AudioTrack(e);
                    _audioTracks = audioTracksQuery.ToArray();
                }
                return _audioTracks;
            }
        }

        /// <summary>
        /// A collection of video tracks contained in the media file.
        /// </summary>
        public IEnumerable<VideoTrack> VideoTracks {
            get {
                if (_videoTracks == null) {
                    var videoTracksQuery =
                        from e in _xml.XPathSelectElements("./me:VideoTracks/me:VideoTrack", _nsm)
                        select new VideoTrack(e);
                    _videoTracks = videoTracksQuery.ToArray();
                }
                return _videoTracks;
            }
        }

        /// <summary>
        /// A collection of names of source media files that were processed in order to produce this output media file.
        /// </summary>
        public IEnumerable<string> Sources {
            get {
                if (_sources == null) {
                    var sourcesQuery =
                        from e in _xml.XPathSelectElements("./me:Sources/me:Source", _nsm)
                        select e.Attribute(XName.Get("Name")).Value;
                    _sources = sourcesQuery.ToArray();
                }
                return _sources;
            }
        }

        /// <summary>
        /// The total bit rate in bits per second, including all video and audio tracks. Counts only the elementary stream payload, and does not include the packaging overhead.
        /// </summary>
        public int Bitrate {
            get {
                var totalVideoBitrate = _videoTracks.Select(videoTrack => videoTrack.Bitrate).Sum();
                var totalAudioBitrate = _audioTracks.Select(audioTrack => audioTrack.Bitrate).Sum();
                return totalVideoBitrate + totalVideoBitrate;
            }
        }

        /// <summary>
        /// The mime type of the asset file.
        /// </summary>
        public string MimeType {
            get {
                return _mimeTypeProvider.GetMimeType(Name);
            }
        }

        /// <summary>
        /// A direct URL to download the asset file using a private locator.
        /// </summary>
        [JsonIgnore]
        public string PrivateUrl {
            get {
                if (!String.IsNullOrEmpty(_parentMetadata.PrivateLocatorUrl)) {
                    var builder = new UriBuilder(_parentMetadata.PrivateLocatorUrl);
                    builder.Path += "/" + Name;
                    return builder.Uri.AbsoluteUri;
                }
                return null;
            }
        }

        /// <summary>
        /// A direct URL to download the asset file using a public locator.
        /// </summary>
        public string PublicUrl {
            get {
                if (!String.IsNullOrEmpty(_parentMetadata.PublicLocatorUrl)) {
                    var builder = new UriBuilder(_parentMetadata.PublicLocatorUrl);
                    builder.Path += "/" + Name;
                    return builder.Uri.AbsoluteUri;
                }
                return null;
            }
        }
    }
}