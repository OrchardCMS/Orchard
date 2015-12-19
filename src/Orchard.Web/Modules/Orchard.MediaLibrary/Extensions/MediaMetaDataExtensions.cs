using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentManagement.MetaData {
    public static class MediaMetaDataExtensions {
        /// <summary>
        /// This extension method can be used for easy image part creation. Adds all necessary parts and settings to the part.
        /// </summary>
        public static ContentTypeDefinitionBuilder AsImage(this ContentTypeDefinitionBuilder builder) {
            return builder
                .AsMedia()
                .WithPart("ImagePart");
        }

        /// <summary>
        /// This extension method can be used for easy vector image part creation. Adds all necessary parts and settings to the part.
        /// </summary>
        public static ContentTypeDefinitionBuilder AsVectorImage(this ContentTypeDefinitionBuilder builder) {
            return builder
                .AsMedia()
                .WithPart("VectorImagePart");
        }

        /// <summary>
        /// This extension method can be used for easy audio part creation. Adds all necessary parts and settings to the part.
        /// </summary>
        public static ContentTypeDefinitionBuilder AsAudio(this ContentTypeDefinitionBuilder builder) {
            return builder
                .AsMedia()
                .WithPart("AudioPart");
        }

        /// <summary>
        /// This extension method can be used for video image part creation. Adds all necessary parts and settings to the part.
        /// </summary>
        public static ContentTypeDefinitionBuilder AsVideo(this ContentTypeDefinitionBuilder builder) {
            return builder
                .AsMedia()
                .WithPart("VideoPart");
        }

        /// <summary>
        /// This extension method can be used for easy document part creation. Adds all necessary parts and settings to the part.
        /// </summary>
        public static ContentTypeDefinitionBuilder AsDocument(this ContentTypeDefinitionBuilder builder) {
            return builder
                .AsMedia()
                .WithPart("DocumentPart");
        }

        /// <summary>
        /// This extension method can be used for easy media part creation. Adds all necessary parts and settings to the part.
        /// </summary>
        public static ContentTypeDefinitionBuilder AsMedia(this ContentTypeDefinitionBuilder builder) {
            return builder
                .WithSetting("Stereotype", "Media")
                .WithPart("CommonPart")
                .WithPart("MediaPart")
                .WithPart("TitlePart");
        }
    }
}