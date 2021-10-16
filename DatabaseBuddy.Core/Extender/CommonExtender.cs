using System;

namespace DatabaseBuddy.Core.Extender
{
  public static class CommonExtender
  {

    public static bool IsNull(this Object Value)
    {
      return (DBNull.Value == Value || Value == null);
    }

    public static bool IsNotNull(this Object Value)
    {
      return !IsNull(Value);
    }

  }
}
