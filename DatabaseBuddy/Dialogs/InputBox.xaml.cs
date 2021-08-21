using System;
using System.Windows;

namespace DatabaseBuddy.Dialogs
{
  /// <summary>
  /// Interaction logic for InputBox.xaml
  /// </summary>
  public partial class InputBox : Window
  {
    public event EventHandler OkRequested;

    #region [Ctor]
    public InputBox(string WindowCaption, string InputPromt, string preFilledText = "")
    {
      InitializeComponent();
      DataContext = new InputBoxViewModel(WindowCaption, InputPromt, preFilledText);
      if (DataContext is InputBoxViewModel VM)
      {
        VM.CancelRequested += __Window_CancelRequested;
        VM.OkRequested += __Window_OkRequested;
      }
      tbUserInput.Focus();
    }
    #endregion

    #region - private methods -

    #region [__Window_OkRequested]
    private void __Window_OkRequested(object sender, EventArgs e)
    {
      OkRequested?.Invoke(sender, e);
      __Window_CancelRequested(this, null);
    }
    #endregion

    #region [__Window_CancelRequested]
    private void __Window_CancelRequested(object sender, EventArgs e)
    {
      if (DataContext is InputBoxViewModel VM)
      {
        VM.CancelRequested -= __Window_CancelRequested;
        VM.OkRequested -= __Window_OkRequested;
      }
      DataContext = null;
      this.Close();
    }
    #endregion
    #endregion
  }
}
