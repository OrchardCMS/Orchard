using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.UI {
    /// <summary>
    /// Compares two positions based on their respective segments
    /// </summary>
    public class PositionComparer : IComparer<string> {
        /// Segments are separated by dots and can be:
        /// - an integer (negative or positive)
        /// - "before" or "after"
        /// - any string value
        /// e.g., 1.2.3; after.1; 1.after; 1.foo
        public int Compare(string x, string y) {
            if (x == y)
                return 0;

            var xs = x == null ? new string[0] : x.Split('.');
            var ys = y == null ? new string[0] : y.Split('.');
            
            for(var i=0; ;i++) {
                // get current i-th segment value
                var xVal = xs.Length > i ? xs[i] : null;
                var yVal = ys.Length > i ? ys[i] : null;

                // if there is no more segment to read, this is a draw
                if (xVal == null && yVal == null) {
                    return 0;
                }

                // if one is longer, it's greater
                if (i > 0) {
                    if (xVal == null) {
                        return -1;
                    }

                    if (yVal == null) {
                        return 1;
                    }
                }

                var result = CompareSegment(xVal, yVal);
                if(result != 0) {
                    // we found a winner
                    return result;
                }
            }
        }

        /// <summary>
        /// Represents the different types of values a segment can have
        /// </summary>
        private enum SegmentTypes {
            Before = 0,
            NegativeNumber = 1,
            Null = 2,
            Empty = 3,
            PositiveNumber = 4,
            String = 5,
            After = 6
        }
        
        /// <summary>
        /// Returns in which segment type a string is
        /// </summary>
        private static SegmentTypes GetSegmentType(string segment) {
            if (String.Equals(segment, "before", StringComparison.OrdinalIgnoreCase)) {
                return SegmentTypes.Before;
            }

            if (segment == null) {
                return SegmentTypes.Null;
            }

            if (String.IsNullOrWhiteSpace(segment)) {
                return SegmentTypes.Empty;
            }

            if (String.Equals(segment, "after", StringComparison.OrdinalIgnoreCase)) {
                return SegmentTypes.After;
            }

            int result;
            if(int.TryParse(segment, out result)) {
                return result < 0 ? SegmentTypes.NegativeNumber : SegmentTypes.PositiveNumber;
            }

            return SegmentTypes.String;
        }

        /// <summary>
        /// Compares two specific segments from a position
        /// </summary>
        /// <returns>-1 if <paramref name="x"/> is greater than <paramref name="y"/>, 0 if they are equal, otherwise -1</returns>
        private static int CompareSegment(string x, string y) {
            // handle raw equality
            // "before", "-42", null, "", "0", "42", "foo", "after"
            if (x == y || String.Equals(x, y, StringComparison.OrdinalIgnoreCase)) {
                return 0;
            }

            var typeX = GetSegmentType(x);
            var typeY = GetSegmentType(y);

            // different types ? we can infer the result
            if(typeX != typeY) {
                return typeX.CompareTo(typeY);
            }

            // same types ? then compare raw values
            switch (typeX) {
                case SegmentTypes.After:
                case SegmentTypes.Before:
                case SegmentTypes.Null:
                case SegmentTypes.Empty:
                    return 0;
                case SegmentTypes.String:
                    return String.Compare(x, y, StringComparison.OrdinalIgnoreCase);
                case SegmentTypes.NegativeNumber:
                case SegmentTypes.PositiveNumber:
                    int xVal, yVal;

                    int.TryParse(x, out xVal);
                    int.TryParse(y, out yVal);

                    return xVal.CompareTo(yVal);

                default:
                    throw new ArgumentException("Unexpected segment type: " + typeX);
                
            }
        }

        /// <summary>
        /// Computes the position right after another one
        /// </summary>
        /// <param name="position">The position to take into account</param>
        /// <returns>Another position, right after the one specified</returns>
        public static string After(string position) {
            if(String.IsNullOrWhiteSpace(position)) {
                return "1";
            }

            var major = position.Split('.')[0];

            int val;
            if(int.TryParse(major, out val)) {
                // integer
                return (val + 1).ToString();
            }
            else {
                if (String.Equals(major, "before", StringComparison.OrdinalIgnoreCase)) {
                    return (int.MinValue + 1).ToString();
                }
                else if (String.Equals(major, "after", StringComparison.OrdinalIgnoreCase)) {
                    return "after";
                }
                else {
                    return major.Trim() + "1";
                }
            }
        }

        /// <summary>
        /// Computes the position next after another one
        /// </summary>
        /// <param name="position">The position to take into account</param>
        /// <returns>Another position, right after the one specified</returns>
        public static string Before(string position) {
            if (String.IsNullOrWhiteSpace(position)) {
                return "-1";
            }

            var segment = position.Split('.')[0];
            int val;
            if (int.TryParse(segment, out val)) {
                // integer
                return (val - 1).ToString();
            } else {
                if (String.Equals(segment, "before", StringComparison.OrdinalIgnoreCase)) {
                    return "before";
                } else if (String.Equals(segment, "after", StringComparison.OrdinalIgnoreCase)) {
                    return (int.MaxValue - 1).ToString();
                } else {
                    return int.MaxValue.ToString();
                }
            }
        }

        /// <summary>
        /// Return a value between two positions
        /// </summary>
        public static string Between(string position1, string position2) {

            if(new PositionComparer().Compare(position1, position2) == 0) {
                // if they are equal, return the same value
                return position1;
            }

            // search for the first segment which is different

            var xs = position1 == null ? new string[0] : position1.Split('.');
            var ys = position2 == null ? new string[0] : position2.Split('.');

            var i=-1;
            string xVal, yVal;
            do {
                // get current i-th segment value
                i++;
                xVal = xs.Length > i ? xs[i] : null;
                yVal = ys.Length > i ? ys[i] : null;
            } while (0 == CompareSegment(xVal, yVal));

            var prefix1 = String.Empty;

            if (i > 0) {
                prefix1 = String.Join(".", xs.Take(i - 1).ToArray());
            }

            // computes the mean value
            var min = Min(xVal, yVal);
            var max = Max(xVal, yVal);

            // if values are adjacent, add a new segment to the first one
            if (After(min) == max) {
                return Min(position1, position2) + ".0";
            }

            // exlude edge cases
            // it's safe as the values are different, and in the worse case they are 
            // following values (before/int.MinValue;int.MaxValue/after)
            if(min == "before") {
                min = After("before");
            }

            if(max == "after") {
                max = Before("after");
            }

            int minInt;

            if(int.TryParse(min, out minInt)) {
                int maxInt;
                if(int.TryParse(max, out maxInt)) {
                    //both numbers
                    return prefix1 + Math.Floor(minInt / 2d + maxInt / 2d);
                }
                else {
                    // max is not integer
                    return prefix1 + Math.Floor(minInt / 2d + int.MaxValue / 2d);
                }
            }

            // two strings
            return Min(position1, position2) + ".0";
        }

        /// <summary>
        /// Returns the greater value of several positions
        /// </summary>
        public static string Max(params string[] positions) {
            return Max((IEnumerable<string>)positions);
        }

        /// <summary>
        /// Returns the greater value of several positions
        /// </summary>
        public static string Max(IEnumerable<string> positions) {
            var posArray = positions.ToArray();

            if (posArray.Length == 0) {
                throw new ArgumentException("positions cannot be empty");
            }

            if (posArray.Length == 1) {
                return posArray[0];
            }

            if (posArray.Length == 2) {
                return new PositionComparer().Compare(posArray[0], posArray[1]) < 0 ? posArray[1] : posArray[0];
            }

            return posArray.Aggregate(posArray[0], (previous, position) => Max(previous, position));
        }

        /// <summary>
        /// Returns the lower value of several positions
        /// </summary>
        public static string Min(params string[] positions) {
            return Min((IEnumerable<string>)positions);
        }

        /// <summary>
        /// Returns the lower value of several positions
        /// </summary>
        public static string Min(IEnumerable<string> positions) {
            var posArray = positions.ToArray();

            if (posArray.Length == 0) {
                throw new ArgumentException("positions cannot be empty");
            }

            if (posArray.Length == 1) {
                return posArray[0];
            }

            if (posArray.Length == 2) {
                return new PositionComparer().Compare(posArray[0], posArray[1]) < 0 ? posArray[0] : posArray[1];
            }

            return posArray.Aggregate(posArray[0], (previous, position) => Max(previous, position));
        }

    }
}