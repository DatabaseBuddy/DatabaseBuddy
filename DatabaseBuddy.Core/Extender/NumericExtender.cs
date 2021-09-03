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

    #region [ToDouble]
    public static double ToDoubleValue(this double? Value)
    {
      if (!Value.HasValue)
        return 0;

      return Value.Value;
    }
    public static double ToDoubleValue(this Object Value, double DefaultValue)
    {
      if (Value == null || DBNull.Value == Value)
        return DefaultValue;

      double Result;

      if (double.TryParse(Value.ToString(), out Result))
        return Result;
      else
        return DefaultValue;
    }
    public static double ToDoubleValue(this Object Value)
    {
      return ToDoubleValue(Value, 0.0d);
    }
    #endregion

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
    public static long ByteToMegabyte(this long Value)
        => (long)(Value / Math.Pow(10, 6));
    public static double ByteToGigabyte(this long Value)
    => (double)Math.Round(Value / Math.Pow(10, 9), 2);
    public static long MegaByteToByte(this long Value)
      => (long)(Value * Math.Pow(10, 6));
  }
}
