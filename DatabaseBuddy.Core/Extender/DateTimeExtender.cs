using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBuddy.Core.Extender
{
    public static class DateTimeExtender
    {
        public static DateTime ToDateTimeValue(this object Value)
        {
            var DefaultValue = DateTime.Now;
            if (Value == null || DBNull.Value == Value)
                return DefaultValue;

            if (Value is DateTime)
                return (DateTime)Value;
            if (Value is DateTime?)
            {
                var xValue = Value as DateTime?;
                if (xValue != null)
                    return xValue.Value;
            }

            return DateTime.TryParse(Value.ToString(), out var Result) ? Result : DefaultValue;
        }
    }
}
