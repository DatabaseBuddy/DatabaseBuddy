using System;
using System.Windows.Input;

namespace DatabaseBuddy.Core
{
  public class DelegateCommand<T> : ICommand where T : class
  {
    private readonly Predicate<T> _canExecute;
    private readonly Action<T> _execute;

    #region [Ctor]
    public DelegateCommand(Action<T> execute) : this(execute, null)
    {
    }
    public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
    {
      _execute = execute;
      _canExecute = canExecute;
    }
    #endregion

    #region - public methods -
    #region [CanExecute]
    public bool CanExecute(object parameter)
    {
      if (_canExecute == null)
        return true;
      return _canExecute((T)parameter);
    }
    #endregion
    #region [Execute]
    public void Execute(object parameter)
    {
      _execute((T)parameter);
    }
    #endregion
    public event EventHandler CanExecuteChanged;
    #region [RaiseCanExecuteChanged]
    public void RaiseCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion
    #endregion
  }
}
