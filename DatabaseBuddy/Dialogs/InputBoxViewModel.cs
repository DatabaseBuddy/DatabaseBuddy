using DatabaseBuddy.Core;
using System;
using System.Windows.Input;
using System.Windows.Markup;

namespace DatabaseBuddy.Dialogs
{
    public class InputBoxViewModel : ViewModelBase
    {
        public event EventHandler CancelRequested;
        public event EventHandler OkRequested;
        private string m_WindowCaption;
        private string m_InputPrompt;
        private string m_UserInput;

        #region [Ctor]
        public InputBoxViewModel(string tmpWindowCaption, string tmpInputPromt, string preFilledText)
        {
            m_WindowCaption = tmpWindowCaption;
            m_InputPrompt = tmpInputPromt;
            UserInput = preFilledText;
            __InitializeCommands();
        }
        #endregion

        #region - properties -
        #region - public properties -
        public string WindowCaption => m_WindowCaption;
        public string InputPrompt => m_InputPrompt;
        public string UserInput
        {
            get => m_UserInput ?? string.Empty;
            set
            {
                m_UserInput = value;
                OnPropertyChanged(nameof(UserInput));
            }
        }

        #endregion
        #endregion

        #region [Icommands]
        public ICommand Cancel { get; set; }
        public ICommand Ok { get; set; }

        #endregion

        #region - commands -
        #region [Execute_Cancel]
        public void Execute_Cancel(object obj = null)
        {
            CancelRequested?.Invoke(this, new EventArgs());
        }
        #endregion

        #region [CanExecute_Ok]
        [DependsOn(nameof(UserInput))]
        public bool CanExecute_Ok() => UserInput.Length > 0;
        #endregion

        #region [Execute_Ok]
        public void Execute_Ok(object obj = null)
        {

            OkRequested?.Invoke(UserInput, new EventArgs());
        }
        #endregion

        #endregion

        private void __InitializeCommands()
        {
            Cancel = new DelegateCommand<object>(Execute_Cancel);
            Ok = new DelegateCommand<object>(Execute_Ok);
        }
    }
}
