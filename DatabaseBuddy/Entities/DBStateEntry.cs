using DatabaseBuddy.Core.Entities;
using DatabaseBuddy.Core.Extender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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
        public Visibility WarningVisible => IsSystemDatabase ? Visibility.Visible : Visibility.Hidden;
        //public Brush TrackedFilesBrush => TrackedFiles.Any() ? Brushes.Green : Brushes.Red;

        public string TrackedFileState
        {
            get
            {
                if (IsTracked)
                  return $"Current Log Size {LDFSize.ToMegabyte()} MB";
                else return "";
            }
        }
        #endregion

        #region - private methods -

        #region [__IsBackupValid]
        private bool __IsBackupValid()
        {
            return DateTime.TryParse(LastBackupTime, out DateTime tryout);
        }
    #endregion

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