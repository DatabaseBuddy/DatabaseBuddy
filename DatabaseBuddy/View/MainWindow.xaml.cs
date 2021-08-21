using DatabaseBuddy.ViewModel;
using System.Windows;

namespace DatabaseBuddy.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        #endregion

    }

}
