using System;

namespace Orchard.Time {
    public class TimeZoneSelectorResult {
        public int Priority { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
    }
}
