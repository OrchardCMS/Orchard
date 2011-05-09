using System.Collections.Generic;
using log4net.Appender;
using log4net.Util;

namespace Orchard.Logging {
    public class OrchardFileAppender : RollingFileAppender {
        /// <summary>
        /// Dictionary of already known suffixes (based on previous attempts) for a given filename.
        /// </summary>
        private static readonly Dictionary<string, int> _suffixes = new Dictionary<string, int>();

        private const int Retries = 50;

        /// <summary>
        /// Opens the log file adding an incremental suffix to the filename if required due to an openning failure (usually, locking).
        /// </summary>
        /// <param name="fileName">The filename as specified in the configuration file.</param>
        /// <param name="append">Boolean flag indicating weather the log file should be appended if it already exists.</param>
        protected override void OpenFile(string fileName, bool append) {
            lock (this) {
                bool fileOpened = false;
                string completeFilename = GetNextOutputFileName(fileName);
                string currentFilename = fileName;

                if (!_suffixes.ContainsKey(completeFilename)) {
                    _suffixes[completeFilename] = 0;
                }

                int newSuffix = _suffixes[completeFilename];

                for (int i = 1; !fileOpened && i <= Retries; i++) {
                    try {
                        if (newSuffix > 0) {
                            currentFilename = string.Format("{0}-{1}", fileName, newSuffix);
                        }

                        BaseOpenFile(currentFilename, append);

                        fileOpened = true;
                    } catch {
                        newSuffix = _suffixes[completeFilename] + i;

                        LogLog.Error(string.Format("OrchardFileAppender: Failed to open [{0}]. Attempting [{1}-{2}] instead.", fileName, fileName, newSuffix));
                    }
                }

                _suffixes[completeFilename] = newSuffix;
            }
        }

        protected virtual void BaseOpenFile(string fileName, bool append) {
            base.OpenFile(fileName, append);
        }
    }
}
