using log4net.Appender;
using log4net.Util;

namespace Orchard.Logging {
    public class OrchardFileAppender : RollingFileAppender {
        public enum RollingStyleFrequencyMode {
            Once,
            Continuous
        }

        public RollingStyleFrequencyMode RollingStyleFrequency { get; set; }

        public bool RollSize {
            get { return (RollingStyle == RollingMode.Once || RollingStyle == RollingMode.Size); }
        }

        protected override void AdjustFileBeforeAppend() {
            if (RollingStyle == RollingMode.Size || 
                RollingStyleFrequency == RollingStyleFrequencyMode.Continuous) {
                base.AdjustFileBeforeAppend();
            }
            else if (RollSize && ((File != null) && (((CountingQuietTextWriter)base.QuietWriter).Count >= MaxFileSize))) {
                RollOverSize();
            }
        }
    }
}
