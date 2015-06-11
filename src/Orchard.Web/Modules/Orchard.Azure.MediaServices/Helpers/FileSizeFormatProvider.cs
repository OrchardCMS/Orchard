using System;

namespace Orchard.Azure.MediaServices.Helpers {
    // Credits: http://blog.flimflan.com/FileSizeFormatProvider.html.
    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter {
        public object GetFormat(Type formatType) {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        private const string FileSizeFormat = "fs";
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        public string Format(string format, object arg, IFormatProvider formatProvider) {
            if (format == null || !format.StartsWith(FileSizeFormat)) {
                return DefaultFormat(format, arg, formatProvider);
            }

            if (arg is string) {
                return DefaultFormat(format, arg, formatProvider);
            }

            Decimal size;

            try {
                size = Convert.ToDecimal(arg);
            }
            catch (InvalidCastException) {
                return DefaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size > OneGigaByte) {
                size /= OneGigaByte;
                suffix = "GB";
            }
            else if (size > OneMegaByte) {
                size /= OneMegaByte;
                suffix = "MB";
            }
            else if (size > OneKiloByte) {
                size /= OneKiloByte;
                suffix = "kB";
            }
            else {
                suffix = " B";
            }

            var precision = format.Substring(2);
            if (String.IsNullOrEmpty(precision)) precision = "2";
            return String.Format("{0:N" + precision + "} {1}", size, suffix);

        }

        private static string DefaultFormat(string format, object arg, IFormatProvider formatProvider) {
            var formattableArg = arg as IFormattable;
            if (formattableArg != null) {
                return formattableArg.ToString(format, formatProvider);
            }
            return arg.ToString();
        }
    }
}