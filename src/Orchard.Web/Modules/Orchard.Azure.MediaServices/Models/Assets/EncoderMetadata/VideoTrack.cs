using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.Azure.MediaServices.Models.Assets.EncoderMetadata {
    public class VideoTrack {

        private readonly XElement _xml;

        public VideoTrack(XElement xml) {
            _xml = xml;
        }

        /// <summary>
        /// The zero-based index of this video track. Note: this is not necessarily the TrackID as used in an MP4 file.
        /// </summary>
        public int Index {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Id")).Value);
            }
        }

        /// <summary>
        /// The average video bit rate in bits per second, as calculated from the media file. Counts only the elementary stream payload, and does not include the packaging overhead.
        /// </summary>
        public int Bitrate {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Bitrate")).Value);
            }
        }

        /// <summary>
        /// The target average bitrate for this video track, as requested in the encoding preset, in bits per second.
        /// </summary>
        public int TargetBitrate {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("TargetBitrate")).Value);
            }
        }

        /// <summary>
        /// The measured video frame rate in frames per second (Hz).
        /// </summary>
        public decimal Framerate {
            get {
                return XmlConvert.ToDecimal(_xml.Attribute(XName.Get("Framerate")).Value);
            }
        }

        /// <summary>
        /// The preset target video frame rate in frames per second (Hz).
        /// </summary>
        public decimal TargetFramerate {
            get {
                return XmlConvert.ToDecimal(_xml.Attribute(XName.Get("TargetFramerate")).Value);
            }
        }

        /// <summary>
        /// The video codec FourCC code.
        /// </summary>
        public string FourCc {
            get {
                return _xml.Attribute(XName.Get("FourCC")).Value;
            }
        }

        /// <summary>
        /// The encoded video width in pixels.
        /// </summary>
        public int Width {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Width")).Value);
            }
        }

        /// <summary>
        /// The encoded video height in pixels.
        /// </summary>
        public int Height {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Height")).Value);
            }
        }

        /// <summary>
        /// The numerator of the video display aspect ratio.
        /// </summary>
        public decimal DisplayAspectRatioX {
            get {
                return XmlConvert.ToDecimal(_xml.Attribute(XName.Get("DisplayAspectRatioNumerator")).Value);
            }
        }

        /// <summary>
        /// The demoninator of the video display aspect ratio.
        /// </summary>
        public decimal DisplayAspectRatioY {
            get {
                return XmlConvert.ToDecimal(_xml.Attribute(XName.Get("DisplayAspectRatioDenominator")).Value);
            }
        }
    }
}