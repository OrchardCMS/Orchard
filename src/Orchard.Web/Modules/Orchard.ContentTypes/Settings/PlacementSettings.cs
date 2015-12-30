using System;

namespace Orchard.ContentTypes.Settings {
    public class PlacementSettings : IEquatable<PlacementSettings> {
        /// <summary>
        /// e.g., Parts_Title_Summary
        /// </summary>
        public string ShapeType { get; set; }

        /// <summary>
        /// e.g., Header, /Navigation
        /// </summary>
        public string Zone { get; set; }
        
        /// <summary>
        /// e.g, 5, after.7
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// e.g, 5, MyTextField
        /// </summary>
        public string Differentiator { get; set; }

        public bool IsSameAs(PlacementSettings other) {
            return (ShapeType ?? String.Empty) == (other.ShapeType ?? String.Empty)
                && (Differentiator ?? String.Empty) == (other.Differentiator ?? String.Empty);
        }

        public bool Equals(PlacementSettings other) {
            if(other == this) {
                return true;
            }

            if(other == null) {
                return false;
            }

            return (ShapeType ?? String.Empty) == (other.ShapeType ?? String.Empty)
                   && (Zone ?? String.Empty) == (other.Zone ?? String.Empty)
                   && (Position ?? String.Empty) == (other.Position ?? String.Empty)
                   && (Differentiator ?? String.Empty) == (other.Differentiator ?? String.Empty);
        }
    }
}