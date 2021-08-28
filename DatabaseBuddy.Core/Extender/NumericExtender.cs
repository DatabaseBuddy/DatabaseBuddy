using System;

namespace DatabaseBuddy.Core.Extender
{
    public static class NumericExtender
    {

        public static long ToLongValue(this object Value, long DefaultValue)
        {
            if (Value == null || DBNull.Value == Value)
                return DefaultValue;

            try
            {
                return Value is string ? long.TryParse(Value.ToString(), out long Result) ? Result : DefaultValue : Convert.ToInt64(Value);
            }
            catch { return DefaultValue; }
        }

        public static long ToLongValue(this long Value)
        {
            return Value;
        }

        public static long ToLongValue(this long? Value)
        {
            return !Value.HasValue ? 0 : Value.Value;
        }
        public static long ToLongValue(this object Value)
        {
            return ToLongValue(Value, 0);
        }

        public static double ToDoubleValue(this object Value)
        {
            return Convert.ToDouble(Value);
        }
        public static int ToInt32Value(this object Value, int DefaultValue = 0)
        {
            if (Value == null || DBNull.Value == Value)
                return DefaultValue;

            try
            {
                if (Value is string)
                {
                    return int.TryParse(Value.ToString(), out int Result) ? Result : DefaultValue;
                }
                else
                {
                    return Convert.ToInt32(Value);
                }
            }
            catch { return DefaultValue; }
        }
        public static long ToMegabyte(this long Value)
            => (long)(Value / Math.Pow(10, 6));
        public static long ToByte(this long Value)
            => (long)(Value * Math.Pow(10, 6));
    }
}
