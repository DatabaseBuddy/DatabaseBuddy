using System;
using System.Windows;

namespace DatabaseBuddy.Core.Events
{
    public class ExecuteCommandRequestedEventArgs : RoutedEventArgs
    {
        public ExecuteCommandRequestedEventArgs(String CommandName, Object CommandParameters)
        {
            this.CommandName = CommandName;
            this.CommandParameters = CommandParameters;
        }

        public String CommandName
        {
            get;
            private set;
        }
        public Object CommandParameters
        {
            get;
            private set;
        }
    }

}