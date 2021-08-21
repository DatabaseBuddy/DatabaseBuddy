using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBuddy.Core.Extender
{
    public static class StringExtender
    {

        public static string StripLeft(this string value, int length)
        {
            return value.Substring(length, value.Length - length);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return value != null && value.Length > 0;
        }

        public static bool IsNullOrEmpty (this string value)
        {
            return !IsNotNullOrEmpty(value);
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

        public static bool ToBooleanValue (this string value)
        {
            return value.Equals("1") ? true : false;
        }

    }
}
