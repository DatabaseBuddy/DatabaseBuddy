using DatabaseBuddy.Core.Entities;
using DatabaseBuddy.Core.Extender;
using DatabaseBuddy.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace DatabaseBuddy.Entities
{
    public class DBStateEntry : DBStateEntryBase
    {
        #region [Ctor]
        public DBStateEntry()
        {
            m_SystemDatabases = new List<string> { "master", "tempdb", "model", "msdb" };
        }
        #endregion

        #region - properties -
        #region - public properties -
        public Visibility SystemDatabaseWarningVisible => IsSystemDatabase ? Visibility.Visible : Visibility.Hidden;
        public Visibility TrackingWarningVisible => LDFSize.ByteToMegabyte() >= MainWindowViewModel.MaxLogSize && MainWindowViewModel.MaxLogSize != 0 ? Visibility.Visible : Visibility.Hidden;
        public string TrackedFileState => $"Current Log Size {LDFSize.ByteToMegabyte()} MB";

        #endregion

        #region - private methods -

        #endregion

        #endregion
    }
    public enum eDATABASESTATE
    {
        OFFLINE,
        ONLINE,
        ALL,
    }
}