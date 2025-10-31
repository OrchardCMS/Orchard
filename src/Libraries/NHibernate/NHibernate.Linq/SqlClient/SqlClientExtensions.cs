using System;

namespace NHibernate.Linq.SqlClient
{
	/// <summary>
	/// Provides static methods that represent functionality provided by MS SQL Server.
	/// </summary>
	public static class SqlClientExtensions
	{
		#region DateTime Functions

		/// <summary>
		/// Returns an integer representing the day datepart of the specified date.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Day(this IDbMethods methods, DateTime value)
		{
			return 0;
		}

		/// <summary>
		/// Returns an integer representing the day datepart of the specified date.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Day(this IDbMethods methods, DateTime? value)
		{
			return 0;
		}

		/// <summary>
		/// Returns an integer that represents the month part of a specified date.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Month(this IDbMethods methods, DateTime value)
		{
			return 0;
		}

		/// <summary>
		/// Returns an integer that represents the month part of a specified date.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Month(this IDbMethods methods, DateTime? value)
		{
			return 0;
		}

		/// <summary>
		/// Returns an integer that represents the year part of a specified date.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Year(this IDbMethods methods, DateTime value)
		{
			return 0;
		}

		/// <summary>
		/// Returns an integer that represents the year part of a specified date.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Year(this IDbMethods methods, DateTime? value)
		{
			return 0;
		}

		#endregion DateTime Functions

		#region Math Functions

		#endregion Math Functions

		#region String Functions

		/// <summary>
		/// Returns the ASCII code value of the leftmost character of a character expression.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Ascii(this IDbMethods methods, string value)
		{
			return 0;
		}

		/// <summary>
		/// Returns the ASCII code value of the leftmost character of a character expression.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Ascii(this IDbMethods methods, char value)
		{
			return 0;
		}

		/// <summary>
		/// Returns the ASCII code value of the leftmost character of a character expression.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Ascii(this IDbMethods methods, char? value)
		{
			return 0;
		}

		/// <summary>
		/// Converts an int ASCII code to a character.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static char Char(this IDbMethods methods, int value)
		{
			return char.MinValue;
		}

		/// <summary>
		/// Converts an int ASCII code to a character.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static char Char(this IDbMethods methods, int? value)
		{
			return char.MinValue;
		}

		/// <summary>
		/// Returns the starting position of the specified expression in a character string.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static int CharIndex(this IDbMethods methods, string value, char search)
		{
			return 0;
		}

		/// <summary>
		/// Returns the starting position of the specified expression in a character string.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="search"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public static int CharIndex(this IDbMethods methods, string value, char search, int start)
		{
			return 0;
		}

		/// <summary>
		/// Returns the starting position of the specified expression in a character string.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static int CharIndex(this IDbMethods methods, string value, string search)
		{
			return 0;
		}

		/// <summary>
		/// Returns the starting position of the specified expression in a character string.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="search"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public static int CharIndex(this IDbMethods methods, string value, string search, int start)
		{
			return 0;
		}

		/// <summary>
		/// Returns the left part of a character string with the specified number of characters.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Left(this IDbMethods methods, string value, int length)
		{
			return null;
		}

		/// <summary>
		/// Returns the number of characters of the specified string expression, excluding trailing blanks.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int Len(this IDbMethods methods, string value)
		{
			return 0;
		}

		/// <summary>
		/// Returns a character expression after converting uppercase character data to lowercase.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Lower(this IDbMethods methods, string value)
		{
			return null;
		}

		/// <summary>
		/// Returns a character expression after it removes leading blanks.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string LTrim(this IDbMethods methods, string value)
		{
			return null;
		}

		/// <summary>
		/// Replaces all occurrences of a specified string value with another string value.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <returns></returns>
		public static string Replace(this IDbMethods methods, string value, string search, string replace)
		{
			return null;
		}

		/// <summary>
		/// Repeats a string value a specified number of times.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static string Replicate(this IDbMethods methods, string value, int count)
		{
			return null;
		}

		/// <summary>
		/// Returns the reverse of a character expression.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Reverse(this IDbMethods methods, string value)
		{
			return null;
		}

		/// <summary>
		/// Returns the right part of a character string with the specified number of characters.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Right(this IDbMethods methods, string value, int length)
		{
			return null;
		}

		/// <summary>
		/// Returns a character string after truncating all trailing blanks.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string RTrim(this IDbMethods methods, string value)
		{
			return null;
		}

		/// <summary>
		/// Returns part of a character, binary, text, or image expression.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Substring(this IDbMethods methods, string value, int start, int length)
		{
			return null;
		}

		/// <summary>
		/// Returns a character expression with lowercase character data converted to uppercase.
		/// </summary>
		/// <param name="methods"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Upper(this IDbMethods methods, string value)
		{
			return null;
		}

		#endregion String Functions
	}
}