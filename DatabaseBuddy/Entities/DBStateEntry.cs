﻿using DatabaseBuddy.Core.Entities;
using DatabaseBuddy.Core.Extender;
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
        public Visibility WarningVisible => IsSystemDatabase ? Visibility.Visible : Visibility.Hidden;
        public string TrackedFileState => $"Current Log Size {LDFSize.ToMegabyte()} MB";
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