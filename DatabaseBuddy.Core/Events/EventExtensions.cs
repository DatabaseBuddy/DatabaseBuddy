using System;
using System.ComponentModel;

namespace DatabaseBuddy.Core.Events
{
    public static class EventExtensions
    {

        public static void Raise(this PropertyChangedEventHandler eventHandler, object source, string propertyName)
        {
            try
            {
                eventHandler?.Invoke(source, new PropertyChangedEventArgs(nameof(propertyName)));
            }
            catch
            {

            }
        }
    }
}
