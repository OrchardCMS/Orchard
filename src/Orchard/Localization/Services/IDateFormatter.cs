using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    public interface IDateFormatter : IDependency {

        /// <summary>
        /// Parses a date/time string into a <c>DateTimeParts</c> instance.
        /// </summary>
        /// <param name="dateTimeString">The date/time string to parse.</param>
        /// <returns></returns>
        DateTimeParts ParseDateTime(string dateTimeString);

        /// <summary>
        /// Parses a date/time string into a <c>DateTimeParts</c> instance using the specified format.
        /// </summary>
        /// <param name="dateTimeString">The date/time string to parse.</param>
        /// <param name="format">A custom DateTime format string with which to parse the string.</param>
        /// <returns></returns>
        DateTimeParts ParseDateTime(string dateTimeString, string format);

        /// <summary>
        /// Parses a date string into a <c>DateParts</c> instance.
        /// </summary>
        /// <param name="dateString">The date string to parse.</param>
        /// <returns></returns>
        DateParts ParseDate(string dateString);

        /// <summary>
        /// Parses a date string into a <c>DateParts</c> instance using the specified format.
        /// </summary>
        /// <param name="dateString">The date string to parse.</param>
        /// <param name="format">A custom DateTime format string with which to parse the string.</param>
        /// <returns></returns>
        DateParts ParseDate(string dateString, string format);

        /// <summary>
        /// Parses a time string into a <c>TimeParts</c> instance.
        /// </summary>
        /// <param name="timeString">The time string to parse.</param>
        /// <returns></returns>
        TimeParts ParseTime(string timeString);

        /// <summary>
        /// Parses a time string into a <c>TimeParts</c> instance using the specified format.
        /// </summary>
        /// <param name="timeString">The date string to parse.</param>
        /// <param name="format">A custom DateTime format string with which to parse the string.</param>
        /// <returns></returns>
        TimeParts ParseTime(string timeString, string format);

        /// <summary>
        /// Formats a <c>DateTimeParts</c> instance into a short date/time string.
        /// </summary>
        /// <param name="parts">The <c>DateTimeParts</c> instance to format.</param>
        /// <returns></returns>
        string FormatDateTime(DateTimeParts parts);

        /// <summary>
        /// Formats a <c>DateTimeParts</c> instance into a string.
        /// </summary>
        /// <param name="parts">The <c>DateTimeParts</c> instance to format.</param>
        /// <param name="format">A custom DateTime format string with which to format the string.</param>
        /// <returns></returns>
        string FormatDateTime(DateTimeParts parts, string format);

        /// <summary>
        /// Formats a <c>DateParts</c> instance into a short date string.
        /// </summary>
        /// <param name="parts">The <c>DateParts</c> instance to format.</param>
        /// <returns></returns>
        string FormatDate(DateParts parts);

        /// <summary>
        /// Formats a <c>DateParts</c> instance into a string.
        /// </summary>
        /// <param name="parts">The <c>DateParts</c> instance to format.</param>
        /// <param name="format">A custom DateTime format string with which to format the string.</param>
        /// <returns></returns>
        string FormatDate(DateParts parts, string format);

        /// <summary>
        /// Formats a <c>TimeParts</c> instance into a long time string.
        /// </summary>
        /// <param name="parts">The <c>DateParts</c> instance to format.</param>
        /// <returns></returns>
        string FormatTime(TimeParts parts);

        /// <summary>
        /// Formats a <c>TimeParts</c> instance into a string.
        /// </summary>
        /// <param name="parts">The <c>TimeParts</c> instance to format.</param>
        /// <param name="format">A custom DateTime format string with which to format the string.</param>
        /// <returns></returns>
        string FormatTime(TimeParts parts, string format);
    }
}
