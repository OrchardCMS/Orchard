using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Orchard.Blogs.Models {
    public class ArchiveData : IEquatable<ArchiveData>, IComparable<ArchiveData> {
        private static readonly string _defaultString = DateTime.Now.Year.ToString();

        private static readonly Regex archiveDataRegex =
            new Regex(@"^(?<year>\d{4})(?:/(?<month>\d{1,2})?(?:/(?<day>\d{1,2})?)?)?(?:/(?:page(?<page>\d+))?)?$",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ArchiveData(string rawData) {
            if (!string.IsNullOrEmpty(rawData)) {
                Match archiveDataMatch = archiveDataRegex.Match(rawData);

                int year;
                if (archiveDataMatch.Groups["year"].Success &&
                    int.TryParse(archiveDataMatch.Groups["year"].Value, out year)) {
                    Year = year;
                }

                int month;
                if (archiveDataMatch.Groups["month"].Success &&
                    int.TryParse(archiveDataMatch.Groups["month"].Value, out month)) {
                    Month = month;
                }

                int day;
                if (archiveDataMatch.Groups["day"].Success &&
                    int.TryParse(archiveDataMatch.Groups["day"].Value, out day)) {
                    Day = day;
                }

                int page;
                if (archiveDataMatch.Groups["page"].Success &&
                    int.TryParse(archiveDataMatch.Groups["page"].Value, out page)) {
                    Page = page;
                }
                else {
                    Page = 1;
                }
            }
        }

        public int Page { get; private set; }
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }

        public static string DefaultString {
            get { return _defaultString; }
        }

        #region IComparable<ArchiveData> Members

        public int CompareTo(ArchiveData other) {
            return ToDateTime().CompareTo(other.ToDateTime());
        }

        #endregion

        #region IEquatable<ArchiveData> Members

        public bool Equals(ArchiveData other) {
            if (other != null) {
                return Day == other.Day && Month == other.Month && Page == other.Page && Year == other.Year;
            }

            return false;
        }

        #endregion

        public override string ToString() {
            var sb = new StringBuilder();

            if (Year > 0) {
                sb.AppendFormat("{0}/", Year);
            }

            if (Month > 0) {
                sb.AppendFormat("{0}/", Month);
            }

            if (Day > 0) {
                sb.AppendFormat("{0}/", Day);
            }

            if (Page > 1) {
                sb.AppendFormat("page{0}/", Page);
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        public DateTime ToDateTime() {
            int projectedYear = DateTime.MinValue.Year, projectedMonth = 1, projectedDay = 1;

            if (Year > 0) {
                projectedYear = Year;
            }
            if (Month > 0) {
                projectedMonth = Month;
            }
            if (Day > 0) {
                projectedDay = Day;
            }

            return new DateTime(projectedYear, projectedMonth, projectedDay);
        }

        public override bool Equals(object obj) {
            if (obj is ArchiveData) {
                return Equals(obj as ArchiveData);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return ToDateTime().GetHashCode();
        }
    }
}