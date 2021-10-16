using System;
using System.Windows.Input;

namespace DatabaseBuddy.Core
{
  public class RelayCommand : ICommand
  {
    #region Fields
    readonly Action<object> _execute;
    readonly Predicate<object> _canExecute;
    #endregion
    #region Constructors
    public RelayCommand(Action<object> execute)
        : this(execute, null)
    {

    }
    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute");

      _execute = execute;
      _canExecute = canExecute;
    }

    #endregion
    #region ICommand Members

    public bool CanExecute(object parameter)
    {
      return _canExecute == null ? true : _canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
      if (CanExecute(parameter))
        _execute(parameter);
    }

    #endregion
    public void OnCanExecuteChanged()
    {
      if (CanExecuteChanged != null)
        CanExecuteChanged(this, EventArgs.Empty);
    }
  }
}