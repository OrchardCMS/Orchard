using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Framework.Localization.Models {
    public struct TimeParts {

        public TimeParts(int hour, int minute, int second, int millisecond) {
            _hour = hour;
            _minute = minute;
            _second = second;
            _millisecond = millisecond;
        }

        private readonly int _hour;
        private readonly int _minute;
        private readonly int _second;
        private readonly int _millisecond;

        public int Hour {
            get {
                return _hour;
            }
        }

        public int Minute {
            get {
                return _minute;
            }
        }

        public int Second {
            get {
                return _second;
            }
        }

        public int Millisecond {
            get {
                return _millisecond;
            }
        }
    }
}
