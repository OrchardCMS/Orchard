using System;
using System.Collections.Generic;

namespace TTS.TalaveraBase.Gravatar {

    /// <summary>
    /// NGravatar avatar rating.
    /// </summary>
    public enum Rating {
        /// <summary>
        /// G
        /// </summary>
        G,
        /// <summary>
        /// PG
        /// </summary>
        PG,
        /// <summary>
        /// R
        /// </summary>
        R,
        /// <summary>
        /// X
        /// </summary>
        X
    }

    /// <summary>
    /// Object that renders Gravatar avatars.
    /// </summary>
    public class Gravatar {

        private static readonly int MinSize = 1;
        private static readonly int MaxSize = 512;

        private int _Size = 80;
        private Rating _MaxRating = Rating.PG;

        /// <summary>
        /// The default image to be shown if no Gravatar is found for an email address.
        /// </summary>
        public string DefaultImage { get; set; }

        /// <summary>
        /// The size, in pixels, of the Gravatar to render.
        /// </summary>
        public int Size {
            get { return _Size; }
            set {
                if (value < MinSize || value > MaxSize)
                    throw new ArgumentOutOfRangeException("Size", "The allowable range for 'Size' is '" + MinSize + "' to '" + MaxSize + "', inclusive.");
                _Size = value;
            }
        }

        /// <summary>
        /// The maximum Gravatar rating allowed to display.
        /// </summary>
        public Rating MaxRating {
            get { return _MaxRating; }
            set { _MaxRating = value; }
        }

        /// <summary>
        /// Creates an img tag whose source is the address of the Gravatar for the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <returns>An HTML img tag of the rendered Gravatar.</returns>
        public string Render(string email) {
            return Render(email, null);
        }

        /// <summary>
        /// Gets a link to the image file of the Gravatar for the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="email">The email whose Gravatar image source should be returned.</param>
        /// <returns>The URI of the Gravatar for the specified <paramref name="email"/>.</returns>
        public string GetImageSource(string email) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(email.Trim()))
                throw new ArgumentException("The email is empty.", "email");

            var imageUrl = "http://www.gravatar.com/avatar.php?";
            var hash = new Gremail(email).Hash();

            imageUrl += "gravatar_id=" + hash;
            imageUrl += "&rating=" + MaxRating.ToString();
            imageUrl += "&size=" + Size.ToString();

            var defImg = DefaultImage;
            if (!string.IsNullOrEmpty(defImg))
                imageUrl += "&default=" + System.Web.HttpUtility.UrlEncode(defImg);

            return imageUrl;
        }

        /// <summary>
        /// Creates an img tag whose source is the address of the Gravatar for the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the img tag.</param>
        /// <returns>An HTML img tag of the rendered Gravatar.</returns>
        public string Render(string email, IDictionary<string, string> htmlAttributes) {
            var imageUrl = GetImageSource(email);

            var attrs = "";
            if (htmlAttributes != null) {
                htmlAttributes.Remove("src");
                htmlAttributes.Remove("width");
                htmlAttributes.Remove("height");
                foreach (var kvp in htmlAttributes)
                    attrs += kvp.Key + "=\"" + kvp.Value + "\" ";
            }

            var img = "<img " + attrs;
            img += "src=\"" + imageUrl + "\" ";
            img += "width=\"" + Size + "\" ";
            img += "height=\"" + Size + "\" ";
            img += "/>";

            return img;
        }
    }
}
