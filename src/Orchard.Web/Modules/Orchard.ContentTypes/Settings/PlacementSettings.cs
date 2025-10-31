using System;

namespace Orchard.ContentTypes.Settings
{
    public class PlacementSettings : IEquatable<PlacementSettings>
    {
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

        public bool IsSameAs(PlacementSettings other)
        {
            return (ShapeType ?? string.Empty) == (other.ShapeType ?? string.Empty)
                && (Differentiator ?? string.Empty) == (other.Differentiator ?? string.Empty);
        }

        public bool Equals(PlacementSettings other)
        {
            if (other == this)
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return (ShapeType ?? string.Empty) == (other.ShapeType ?? string.Empty)
                   && (Zone ?? string.Empty) == (other.Zone ?? string.Empty)
                   && (Position ?? string.Empty) == (other.Position ?? string.Empty)
                   && (Differentiator ?? string.Empty) == (other.Differentiator ?? string.Empty);
        }
    }
}