using System;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseBuddy.Core.OperationVisualization
{
    public enum eOperationState
    {
        None = 0,
        Running = 1,
        Successful = 2,
        Failed = 3,
        Warning = 4,
        Information = 5,
        Started = 6,
    }


    public partial class OperationState : UserControl
    {
        #region Constants
        private const String STATE_NONE = nameof(None);
        private const String STATE_RUNNING = nameof(Running);
        private const String STATE_SUCCESSFUL = nameof(Successful);
        private const String STATE_FAILED = nameof(Failed);
        private const String STATE_WARNING = nameof(Warning);
        private const String STATE_INFORMATION = nameof(Information);
        private const String STATE_STARTED = nameof(Started);
        #endregion


        #region Constructor
        public OperationState()
        {
            InitializeComponent();
        }
        #endregion

        #region OperationState

        public static readonly DependencyProperty OperationStateProperty = DependencyProperty.Register("eOperationState", typeof(eOperationState), typeof(OperationState), new PropertyMetadata(eOperationState.None, OnOperationStatePropertyChanged));

        public static void OnOperationStatePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (SuppressDependencyPropertyChangedEventScope.SuppressChangedEvent(OperationStateProperty))
                return;

            OperationState Sender = o as OperationState;
            Sender.OneOperationStateChanged((eOperationState)e.OldValue, (eOperationState)e.NewValue);
        }

        public eOperationState eOperationState
        {
            get
            {
                return (eOperationState)GetValue(OperationStateProperty);
            }
            set
            {
                SetValue(OperationStateProperty, value);
            }
        }

        protected virtual void OneOperationStateChanged(eOperationState OldValue, eOperationState NewValue)
        {
            if (OldValue != NewValue)
                UpdateVisualState(NewValue);
        }
        #endregion

        #region Percentage
        public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register("Percentage", typeof(int), typeof(OperationState), new PropertyMetadata(0, OnPercentagePropertyChanged));

        public static void OnPercentagePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (SuppressDependencyPropertyChangedEventScope.SuppressChangedEvent(PercentageProperty))
                return;

            OperationState Sender = o as OperationState;
            Sender.OnPercentagehanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual void OnPercentagehanged(int OldValue, int NewValue)
        {
            if (NewValue > 0)
            {
                PercentageText.Text = NewValue.ToString();

                if (NewValue > 99)
                    PercentageText.Text = "99";

                PercentageText.Visibility = Visibility.Visible;
            }
            else
                PercentageText.Visibility = Visibility.Collapsed;
        }

        public int Percentage
        {
            get { return (int)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }
        #endregion

        #region - helper -

        private void UpdateVisualState(eOperationState State)
        {
            switch (State)
            {
                case eOperationState.None:
                    GotoState(STATE_NONE);
                    break;
                case eOperationState.Running:
                    GotoState(STATE_RUNNING);
                    break;
                case eOperationState.Successful:
                    GotoState(STATE_SUCCESSFUL);
                    break;
                case eOperationState.Failed:
                    GotoState(STATE_FAILED);
                    break;
                case eOperationState.Warning:
                    GotoState(STATE_WARNING);
                    break;
                case eOperationState.Information:
                    GotoState(STATE_INFORMATION);
                    break;
                case eOperationState.Started:
                    GotoState(STATE_STARTED);
                    break;
            }
        }

        private void GotoState(String StateName)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                VisualStateManager.GoToState(this, StateName, false);
                this.UpdateLayout();
            }));

        }

        #endregion
    }
}
