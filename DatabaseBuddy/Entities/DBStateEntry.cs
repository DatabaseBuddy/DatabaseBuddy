﻿using DatabaseBuddy.Core.OperationVisualization;
using Övervakning.Shared.Entities;
using Övervakning.Shared.Enums;
using Övervakning.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DatabaseBuddy.Entities
{
    public class DBStateEntry
    {
        private List<string> m_systemDataBases;
        #region [Ctor]
        public DBStateEntry()
        {
            m_systemDataBases = new List<string> { "master", "tempdb", "model", "msdb" };
            TrackedFiles = new List<Entry>();
        }
        #endregion

        #region - properties -
        #region - public properties -
        public bool CanDBRestore => __IsBackupValid();
        public string CutLogfileText => $"Cut Log Size: {LDFSize} MB";
        public bool RestrictedRights => !IsSystemDataBase;
        public string DBName { get; set; }

        public string CloneName { get; set; }
        public string HandleStatusText => IsOnline ? "Take offline" : "Take online";
        public string HandleODBCState => HasODBCEntry ? "Delete ODBC" : "Create ODBC Entry";
        #endregion

        public eOperationState OperationState => IsOnline ? eOperationState.Successful : eOperationState.Failed;
        public string StateTooltip => IsOnline ? "Online" : "Offline";
        public bool IsOnline
        {
            get;
            set;
        }
        public bool IsSelected { get; set; }
        public bool ForMultiMode { get; set; }
        public bool IsSystemDataBase => m_systemDataBases.Contains(DBName);
        public Visibility WarningVisible => IsSystemDataBase ? Visibility.Visible : Visibility.Hidden;
        public string LastBackupPath { get; set; }
        public string LastBackupTime { get; set; }
        public string LDFLocation { get; set; }
        public long LDFSize { get; set; }
        public long MDFSize { get; set; }
        public long DataBaseSize => MDFSize + LDFSize;
        public string DataBaseSizeString => $"Data File: {MDFSize} MB \n Log Size: {LDFSize} MB \n Sum: {DataBaseSize} MB";
        public string MDFLocation { get; set; }
        public string RestoreBackupCaption
        {
            get
            {
                var ValidBackup = __IsBackupValid();
                if (LastBackupTime.Length > 0)
                {
                    if (!ValidBackup)
                        return LastBackupTime;
                    return $"Restore last Backup ({LastBackupTime})";
                }
                return "Found no Backup";
            }
        }
        public bool HasODBCEntry { get; set; }
        public List<Entry> TrackedFiles { get; set; }
        public Brush TrackedFilesBrush => TrackedFiles.Any() ? Brushes.Green : Brushes.Red;

        public string TrackedFileState
        {
            get
            {
                if (TrackedFiles.Any())
                {
                    var File = TrackedFiles.FirstOrDefault();
                    var ConvertedSize = DataDimensionConverter.Convert(LDFSize, eDataDimension.Megabyte, File.DataDimension);
                    return $"Current Log Size {LDFSize} MB: {ConvertedSize} {File.DataDimension} | Max Size {TrackedFiles.FirstOrDefault().MaxFileSize} {File.DataDimension}";
                }
                else
                {
                    return "No Tracking Activated";
                }
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
    }
    public enum eDATABASESTATE
    {
        OFFLINE,
        ONLINE,
        ALL,
    }
}