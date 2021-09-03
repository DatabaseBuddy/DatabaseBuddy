using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBuddy.Core.Extender
{
    public static class ListExtender
    {
        public static void RemoveFrom<T>(this List<T> lst, int from)
        {
            lst.RemoveRange(from, lst.Count - from);
        }
    }
}
