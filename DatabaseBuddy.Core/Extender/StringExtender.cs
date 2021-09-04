using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseBuddy.Core.Extender
{
  public static class StringExtender
  {

    public static string StripLeft(this string value, int length)
    {
      if (length < 0 || value.Length < length)
        return string.Empty;
      return value.Substring(length, value.Length - length);
    }

    public static bool IsNotNullOrEmpty(this string value)
    {
      return value != null && value.Length > 0;
    }

    public static bool IsNullOrEmpty(this string value)
    {
      return !IsNotNullOrEmpty(value);
    }

    public static bool IsNullOrEmptyOrWhiteSpace(this string value)
    {
      return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
    }
    public static bool IsNotNullOrEmptyOrWhiteSpace(this string value)
    {
      return !IsNullOrEmptyOrWhiteSpace(value);
    }
    public static bool ContainsOnlyWhiteSpace(this string value)
    {
      return value.IsNotNullOrEmpty() && value.All(x => x.Equals(' '));
    }
    public static bool ContainsNotOnlyWhiteSpace(this string value)
    {
      return !ContainsOnlyWhiteSpace(value);
    }

    public static string ToStringValue(this object Value)
    {
      return ToStringValue(Value, string.Empty);
    }

    public static string ToStringValue(this object Value, string DefaultValue)
    {
      return ToStringValue(Value, DefaultValue, CultureInfo.CurrentCulture);
    }

    public static string ToStringValue(this object Value, string DefaultValue, CultureInfo Culture)
    {
      if (Value == null || DBNull.Value == Value)
        return DefaultValue;

      try
      {
        return Convert.ToString(Value, Culture);
      }
      catch { return DefaultValue; }
    }

    public static bool ToBooleanValue(this string value)
    {
      return value.IsNotNullOrEmptyOrWhiteSpace() && (value.Equals("1") || value.ToLower().Equals("true"));
    }

    public static bool IsValidIPAddress(this string value)
    {
      if (value.IsNullOrEmpty() || string.IsNullOrWhiteSpace(value))
        return false;
      return Regex.IsMatch(value, "(([0-255]\\.[0-255]\\.[0-255]\\.[0-255])|(0{1,3}\\.0{1,3}\\.0{1,3}\\.0{1,3}))");
    }
  }
}
