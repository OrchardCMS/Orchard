using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Orchard.Azure.MediaServices.Models.Assets.EncoderMetadata {
    public class AudioTrack {

        private readonly XElement _xml;

        public AudioTrack(XElement xml) {
            _xml = xml;
        }

        /// <summary>
        /// The zero-based index of the audio track. Note: this is not necessarily the TrackID as used in an MP4 file.
        /// </summary>
        public int Index {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Id")).Value);
            }
        }

        /// <summary>
        /// The average audio bitrate in bits per second, as calculated from the media file. Takes into consideration only the elementary stream payload and does not include the packaging overhead.
        /// </summary>
        public int Bitrate {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Bitrate")).Value);
            }
        }

        /// <summary>
        /// The audio sampling rate in samples/sec or Hz
        /// </summary>
        public int SamplingRate {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("SamplingRate")).Value);
            }
        }

        /// <summary>
        /// The bits per sample for the audio track.
        /// </summary>
        public int BitsPerSample {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("BitsPerSample")).Value);
            }
        }

        /// <summary>
        /// The number of audio channels in the audio track.
        /// </summary>
        public int Channels {
            get {
                return XmlConvert.ToInt32(_xml.Attribute(XName.Get("Channels")).Value);
            }
        }

        /// <summary>
        /// The audio codec used for encoding the audio track.
        /// </summary>
        public string Codec {
            get {
                return _xml.Attribute(XName.Get("Codec")).Value;
            }
        }

        /// <summary>
        /// The optional encoder version string, required for EAC3.
        /// </summary>
        public string EncoderVersion {
            get {
                var encoderVersionAttribute = _xml.Attribute(XName.Get("EncoderVersion"));
                if (encoderVersionAttribute != null)
                    return encoderVersionAttribute.Value;
                return null;
            }
        }
    }
}