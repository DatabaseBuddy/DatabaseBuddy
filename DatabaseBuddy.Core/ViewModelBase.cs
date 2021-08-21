using DatabaseBuddy.Core.Attributes;
using DatabaseBuddy.Core.Events;
using DatabaseBuddy.Core.Extender;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DatabaseBuddy.Core
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
      
        public ViewModelBase()
        { 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}