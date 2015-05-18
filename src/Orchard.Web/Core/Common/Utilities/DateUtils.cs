using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Core.Common.Utilities {
    public class DateUtils {

        /// <summary>
        /// Compares two <see cref="DateTime" /> instance without their milliseconds portion.
        /// </summary>
        /// <param name="a">The first <see cref="DateTime" /> to compare.</param>
        /// <param name="b">The second <see cref="DateTime" /> to compare.</param>
        /// <returns><c>True</c> if the two instances are in the same second, <c>False</c> otherwise.</returns>
        public static bool DatesAreEquivalent(DateTime a, DateTime b) {
            a = a.ToUniversalTime();
            b = b.ToUniversalTime();

            return
                new DateTime(a.Year, a.Month, a.Day, a.Hour, a.Minute, a.Second) ==
                new DateTime(b.Year, b.Month, b.Day, b.Hour, b.Minute, b.Second);

        }

        /// <summary>
        /// Compares two <see cref="DateTime?" /> instance without their milliseconds portion.
        /// </summary>
        /// <param name="a">The first <see cref="DateTime?" /> to compare.</param>
        /// <param name="b">The second <see cref="DateTime?" /> to compare.</param>
        /// <returns><c>True</c> if the two instances are in the same second, <c>False</c> otherwise.</returns>
        public static bool DatesAreEquivalent(DateTime? a, DateTime? b) {
            if (!a.HasValue && !b.HasValue) {
                return true;
            }

            if (a.HasValue != b.HasValue) {
                return false;
            }

            return DatesAreEquivalent(a.Value, b.Value);
        }

    }
}