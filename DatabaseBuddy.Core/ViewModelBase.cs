using System.ComponentModel;
using System.Runtime.CompilerServices;

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