using System.Collections.Generic;

namespace DatabaseBuddy.Core.Extender
{
  public static class ListExtender
  {
    public static void RemoveFrom<T>(this List<T> lst, int from)
    {
      if (from < 0)
        return;
      lst.RemoveRange(from, lst.Count - from);
    }
  }
}
