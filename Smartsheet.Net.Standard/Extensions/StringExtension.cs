using System;
namespace Smartsheet.Net.Standard.Extensions
{
	public static class StringExtension
	{
		public static string ToCamelCase (this string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				if (str.Length > 1)
				{
					return Char.ToLowerInvariant(str[0]) + str.Substring(1);
				}
				else
				{
					return str.ToLower();
				}
			}
			return str;
		}
	}
}
