using DatabaseBuddy.ViewModel;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace DatabaseBuddy.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region [Ctor]
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region - private methods -
        #region [__WindowLoaded_Start]
        private void __WindowLoaded_Start(object sender, RoutedEventArgs e)
        {
            this.DataContext = new MainWindowViewModel();
        }
        #endregion

        #region [__ListBoxItem_MouseEnter]
        private void __ListBoxItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!lbDataBases.ContextMenu.IsOpen)
                ((MainWindowViewModel)DataContext).Execute_GotFocus(sender);
        }
        #endregion


        private void __PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).Password = ((PasswordBox)sender).Password;
        }

        #region [__FilterChanged]
        private void __FilterChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DBFilter = ((TextBox)sender).Text;
        }
        #endregion
        #endregion
    }

}
