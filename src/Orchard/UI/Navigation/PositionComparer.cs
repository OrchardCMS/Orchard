using System;
using System.Collections.Generic;

namespace Orchard.UI.Navigation {
    public class PositionComparer : IComparer<string> {
        public int Compare(string x, string y) {
            if (x == null || y == null) {
                return x == null && y == null ? 0 : (x == null ? -1 : 1);
            }
            if (x == "" || y == "") {
                return x == "" && y == "" ? 0 : (x == "" ? -1 : 1);
            }

            var xRange = new Range { Length = x.Length };
            var yRange = new Range { Length = y.Length };

            while (xRange.Start != xRange.Length || yRange.Start != yRange.Length) {
                var xSize = xRange.NextDot(x);
                var ySize = yRange.NextDot(y);

                if (xSize == 0 || ySize == 0) {
                    // one or both sides are empty
                    if (xSize != 0 || ySize != 0) {
                        // favor the side that's not empty
                        return xSize - ySize;
                    }
                    // otherwise continue to the next segment if both are
                }
                else if (xRange.NumericValue != -1 && yRange.NumericValue != -1) {
                    // two strictly numeric values 

                    // return the difference
                    var diff = xRange.NumericValue - yRange.NumericValue;
                    if (diff != 0)
                        return diff;

                    // or continue to next segment
                }
                else {
                    if (xRange.NumericValue != -1) {
                        // left-side only has numeric value, right-side explicitly greater
                        return -1;
                    }

                    if (yRange.NumericValue != -1) {
                        // right-side only has numeric value, left-side explicitly greater
                        return 1;
                    }

                    // two strictly non-numeric
                    var diff = string.Compare(x, xRange.Start, y, yRange.Start, Math.Min(xSize, ySize),
                                              StringComparison.OrdinalIgnoreCase);
                    if (diff != 0)
                        return diff;
                    if (xSize != ySize)
                        return xSize - ySize;
                }

                xRange.Advance();
                yRange.Advance();
            }

            return 0;
        }

        struct Range {
            public int Start { get; private set; }
            private int End { get; set; }
            public int Length { get; set; }

            public int NumericValue { get; private set; }

            public int NextDot(string value) {
                if (Start == -1) {
                    End = -1;
                    return 0;
                }

                End = value.IndexOf('.', Start);

                int numeric;
                NumericValue = int.TryParse(Segment(value), out numeric) ? numeric : -1;

                return End == -1 ? Length - Start : End - Start;
            }

            private string Segment(string value) {
                if (Start == 0 && End == -1) {
                    return value;
                }
                if (End == -1) {
                    return value.Substring(Start);
                }
                return value.Substring(Start, End - Start);
            }

            public void Advance() {
                if (End == -1)
                    Start = Length;
                else
                    Start = End + 1;
            }
        }
    }
}