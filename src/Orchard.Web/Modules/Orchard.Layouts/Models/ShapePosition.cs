using System;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Models {
    public class ShapePosition {
        public int Position { get; set; }
        public string Name { get; set; }

        public static readonly ShapePosition Empty = new ShapePosition();

        public static ShapePosition Parse(string text) {
            if(String.IsNullOrWhiteSpace(text))
                return Empty;

            var parts = text.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);
            var position = new ShapePosition();

            if (parts.Length > 0)
                position.Name = parts[0];

            if (parts.Length > 1) {
                position.Position = parts[1].ToInt32().GetValueOrDefault();
            }

            return position;
        }
    }
}