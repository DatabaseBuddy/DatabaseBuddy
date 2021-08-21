﻿using ASquare.WindowsTaskScheduler;
using DatabaseBuddy.Core;
using DatabaseBuddy.Core.Attributes;
using DatabaseBuddy.Core.DatabaseExtender;
using DatabaseBuddy.Core.Extender;
using DatabaseBuddy.Dialogs;
using DatabaseBuddy.Entities;
using Microsoft.Win32;
using Övervakning.Shared.Entities;
using Övervakning.Shared.Enums;
using Övervakning.Shared.Helper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DatabaseBuddy.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region [needs]
        private List<string> m_systemDataBases;
        private List<string> m_UnusedFiles;
        private string m_Server = "localhost";
        private readonly string m_User; //TODO: TASK:  Replace via Login Dialog
        private readonly string m_Password; //TODO: TASK: Replace via Login Dialog
        private const string ODBC32BitRegPath = @"SOFTWARE\WOW6432Node\ODBC\ODBC.INI";
        private string m_MSSQLStudioPath;
        private DBConnection m_db;
        private string m_DNSName = string.Empty;
        private string m_DNSBase = "{0}_{1}";

        private string m_TrackingMaxFileSizeInput;
        private DBStateEntry m_TrackingDBStateEntry;

        private bool m_ShowSystemDatabases;
        private bool m_FileTrackingEnabled;
        private bool m_ScheduleActivated;
        private List<Entry> m_TrackedFiles;
        bool skipReload;
        private DBStateEntry m_SelectedDB;
        private List<DBStateEntry> m_DBEntries;
        private ListBox m_ListBoxDbs;
        private bool m_IsBusy;
        private bool m_MultiMode;
        #endregion

        #region [Icommands]
        public ICommand Reload { get; set; }
        public ICommand CreateNewDatabase { get; set; }
        public ICommand ShowSystemDatabases { get; set; }
        public ICommand SwitchMultiMode { get; set; }
        public ICommand SelectAll { get; set; }
        public ICommand SwitchTracking { get; set; }
        public ICommand Reconnect { get; set; }
        public ICommand GotLbFocus { get; set; }
        public ICommand DeleteSelectedDataBase { get; set; }
        public ICommand OpenFolder { get; set; }
        public ICommand OpenLastBackupFolder { get; set; }
        public ICommand CloneDataBase { get; set; }
        public ICommand RenameDatabase { get; set; }
        public ICommand CutLogFile { get; set; }
        public ICommand OpenQuery { get; set; }
        public ICommand GenerateODBCEntry { get; set; }
        public ICommand TakeSelectedOffline { get; set; }
        public ICommand BackupSelectedDataBase { get; set; }
        public ICommand RestoreBackup { get; set; }
        public ICommand StartMonitoring { get; set; }
        public ICommand TakeAllOffline { get; set; }
        public ICommand TakeAllOnline { get; set; }
        public ICommand BackupAll { get; set; }
        public ICommand RestoreAll { get; set; }
        public ICommand DeleteUnusedFiles { get; set; }
        public ICommand RestoreMultipleBaks { get; set; }


        #endregion

        #region [Ctor]
        public MainWindowViewModel()
        {
            __InitializeCommands();
            m_systemDataBases = new List<string> { "master", "tempdb", "model", "msdb" };
            UnusedFiles = new List<string>();
            __GetMSSQLStudioPath();
            m_ShowSystemDatabases = GetRegistryValue("ShowSystemDatabases").ToBooleanValue();
            m_FileTrackingEnabled = GetRegistryValue("EnableFileSizeMonitoring").ToBooleanValue();
            m_ScheduleActivated = GetRegistryValue("EnabledSchedule").ToBooleanValue();
            ServerName = GetRegistryValue(nameof(ServerName));
            __HandleFileTrackingNeeds();
            skipReload = false;
            Execute_Reload();
        }
        #endregion

        #region - properties -
        #region - public properties -
        [DependsUpon(nameof(ServerName))]
        public string SystemInformation => $"{__GetVersion()} Server: {m_Server} User: {m_User}";

        public bool MultiMode
        {
            get
            {
                return m_MultiMode;
            }
            private set
            {
                m_MultiMode = value;
                OnPropertyChanged(nameof(MultiMode));
            }
        }

        public List<string> UnusedFiles
        {
            get => m_UnusedFiles;
            private set
            {
                if (m_UnusedFiles == null)
                    m_UnusedFiles = new List<string>();
                if (value != null)
                    m_UnusedFiles = value;
                OnPropertyChanged(nameof(UnusedFiles));
            }
        }

        public List<DBStateEntry> DBEntries
        {
            get => m_DBEntries ?? new List<DBStateEntry>();
            private set
            {
                m_DBEntries = value;
                OnPropertyChanged(nameof(DBEntries));
            }
        }

        public List<DBStateEntry> SelectedDbs => ListBoxDbs.SelectedItems.Cast<DBStateEntry>().ToList();

        public DBStateEntry SelectedDB
        {
            get => m_SelectedDB;
            private set
            {
                m_SelectedDB = value;
                OnPropertyChanged(nameof(SelectedDB));
            }
        }

        [DependsUpon(nameof(SelectedDB))]
        public bool RestrictedRights => !SelectedDB?.IsSystemDataBase ?? false;

        public ListBox ListBoxDbs
        {
            get => m_ListBoxDbs;
            private set
            {
                m_ListBoxDbs = value;
                OnPropertyChanged(nameof(ListBoxDbs));
            }
        }

        public bool IsBusy
        {
            get => m_IsBusy;
            private set
            {
                m_IsBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        [DependsUpon(nameof(UnusedFiles))]
        public string DeleteUnusedFilesCaption => !UnusedFiles.Any() ? "No unused database files" : $"Delete {UnusedFiles.Count} unused database files";

        [DependsUpon(nameof(MultiMode))]
        public SelectionMode ListBoxSelectionMode => MultiMode ? SelectionMode.Multiple : SelectionMode.Single;

        [DependsUpon(nameof(MultiMode))]
        public Visibility SelectAllVisibility => MultiMode ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
      ? Visibility.Collapsed : Visibility.Visible;

        [DependsUpon(nameof(m_FileTrackingEnabled))]
        public Visibility FileMonitoringVisibility => m_FileTrackingEnabled ? Visibility.Visible : Visibility.Collapsed;

        [DependsUpon(nameof(MultiMode))]
        public string MultiModeToggleCaption => MultiMode ? "MULTIMODE ON" : "MULTIMODE OFF";

        public DBConnection Db
        {
            get
            {
                try
                {
                    if (m_db == null)
                    {
                        m_db = new DBConnection(ServerName, "master", m_User, m_Password);
                        m_db.ConnectionFailed += __ResetInvalidConnection;
                    }
                    return m_db;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public string ServerName
        {
            get
            {
                return m_Server ?? "localhost";
            }
            set
            {
                var TrimmedValue = value?.Trim();
                m_Server = TrimmedValue.IsNotNullOrEmpty() ? TrimmedValue : "localhost";
                OnPropertyChanged(nameof(ServerName));
                OnPropertyChanged(nameof(SystemInformation));
            }
        }

        #endregion
        #endregion

        #region - commands -

        #region [CanExecute_TakeSelectedOffline]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_TakeSelectedOffline() => __CanExecute_Common();
        #endregion

        #region [Execute_TakeSelectedOffline]
        public void Execute_TakeSelectedOffline(object sender)
        {
            try
            {
                if (sender is DBStateEntry State)
                {
                    if (State.IsOnline)
                    {
                        if (MultiMode || State.DBName.Equals("master", StringComparison.InvariantCultureIgnoreCase))
                            return;
                        var DataBaseEntries = new List<DBStateEntry> { State };
                        var Messagetext = $"Are you sure to take offline the following databases?\n";
                        DataBaseEntries.ForEach(x => Messagetext += $"-{x.DBName}\n");
                        var KillResult = MessageBox.Show(Messagetext, "Confirm taking offline", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (KillResult == MessageBoxResult.No)
                            return;
                        __KillConnections(DataBaseEntries);
                    }
                    else
                        __ActivateConnections(new List<DBStateEntry> { State });
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Execute_Reload();
            }
        }
        #endregion

        #region [Execute_OpenQuery]
        public void Execute_OpenQuery(object sender)
        {
            try
            {
                if (sender is DBStateEntry State)
                    __OpenMSSQLQuery(State.DBName);
            }
            catch (Exception ex)
            {
            }

        }
        #endregion

        #region [Execute_GenerateODBCEntry]
        public void Execute_GenerateODBCEntry(object sender)
        {
            try
            {
                if (MultiMode && SelectedDbs.Any())
                {
                    __CreateODBCEntries(SelectedDbs);
                }
                else if (sender is DBStateEntry State)
                {
                    if (State.HandleODBCState.Equals("Delete ODBC"))
                        __DeleteODBCEntries(new List<DBStateEntry> { State });
                    else
                        __CreateODBCEntries(new List<DBStateEntry> { State });
                }
            }
            catch (Exception ex)
            {
            }

        }
        #endregion

        #region [Execute_TakeAllOffline]
        public void Execute_TakeAllOffline(object obj = null)
        {
            try
            {
                var DataBaseEntries = __LoadDataBases(eDATABASESTATE.ONLINE);
                var Messagetext = $"Are you sure to take offline the following databases?\n";
                DataBaseEntries.Where(x => !x.IsSystemDataBase).ToList().ForEach(x => Messagetext += $"-{x.DBName}\n");
                var KillResult = MessageBox.Show(Messagetext, "Confirm taking offline", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (KillResult == MessageBoxResult.No)
                    return;
                __KillConnections(DataBaseEntries.Where(x => !x.IsSystemDataBase).ToList());
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_TakeAllOnline]
        public void Execute_TakeAllOnline(object obj = null)
        {
            try
            {
                __ActivateConnections(__LoadDataBases(eDATABASESTATE.OFFLINE));
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_DeleteSelectedDataBase]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_DeleteSelectedDataBase() => __CanExecute_Common();
        #endregion

        #region [Execute_DeleteSelectedDataBase]
        public void Execute_DeleteSelectedDataBase(object obj = null)
        {
            try
            {
                if (MultiMode && SelectedDbs.Any())
                    __DeleteDataBase(SelectedDbs);
                else if (SelectedDB != null)
                    __DeleteDataBase(new List<DBStateEntry> { SelectedDB });
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_GotFocus]
        public void Execute_GotFocus(object sender)
        {
            try
            {

                if (((ListBoxItem)sender).Content is DBStateEntry State)
                    SelectedDB = State;
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_GotLbFocus]
        public void Execute_GotLbFocus(object sender)
        {
            try
            {
                if (sender is ListBox ListBox)
                    ListBoxDbs = ListBox;
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_BackupAll]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_BackupAll() => __CanExecute_Common();
        #endregion

        #region [Execute_BackupAll]
        public void Execute_BackupAll(object obj = null)
        {
            try
            {
                __BackupDatabases(DBEntries);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_RestoreBackup]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_RestoreBackup() => __CanExecute_Common();
        #endregion

        #region [Execute_RestoreBackup]
        public void Execute_RestoreBackup(object obj = null)
        {
            try
            {
                if (MultiMode && SelectedDbs.Any())
                    __RestoreBackup(SelectedDbs);
                else if (SelectedDB != null)
                    __RestoreBackup(new List<DBStateEntry> { SelectedDB });
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_BackupSelectedDataBase]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_BackupSelectedDataBase() => __CanExecute_Common();
        #endregion

        #region [Execute_BackupSelectedDataBase]
        public void Execute_BackupSelectedDataBase(object sender)
        {
            try
            {
                if (sender is DBStateEntry State)
                    __BackupDatabases(new List<DBStateEntry> { State });
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_RestoreAll]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_RestoreAll() => __CanExecute_Common();
        #endregion

        #region [Execute_RestoreAll]
        public void Execute_RestoreAll(object obj = null)
        {
            try
            {
                __RestoreBackup(DBEntries);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_Reload]
        public void Execute_Reload(object obj = null)
        {
            try
            {
                if (skipReload)
                    return;
                //DBEntries.Clear();
                Db.ExecuteScalar("USE [master] SELECT TOP (1) xserver_name FROM [master].[dbo].[spt_fallback_db]");
                __ReloadDBs();
                __GetUnusedDataBaseFiles();
                __SetMultiMode(false);
                __AssignTrackedFiles();
                __AssignGeneralProps();
                if (ListBoxDbs != null)
                {
                    ListBoxDbs.ItemsSource = null;
                    ListBoxDbs.ItemsSource = DBEntries;
                }
            }
            catch (Exception ex)
            {
                __ResetInvalidConnection(this, new EventArgs());
                //_ = MessageBox.Show(ex.ToString()/*"Datenbankverbindung fehlgeschlagen"*/, "Verbindung nicht möglich");
            }
        }

        private void __ResetInvalidConnection(object sender, EventArgs e)
        {
            skipReload = true;
            ServerName = GetRegistryValue(nameof(ServerName));
            m_db = null;
            Execute_Reload();
        }
        #endregion

        #region [Execute_DeleteUnusedFiles]
        public void Execute_DeleteUnusedFiles(object obj = null)
        {
            try
            {
                if (!UnusedFiles.Any())
                    return;
                var Messagetext = $"Are you sure to delete the following {UnusedFiles.Count} unused Files?\n";
                UnusedFiles.ForEach(x => Messagetext += $"-{x}\n");
                var DeleteResult = MessageBox.Show(Messagetext, "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (DeleteResult == MessageBoxResult.No)
                    return;
                else
                {
                    UnusedFiles.ForEach(File.Delete);
                    Execute_Reload();
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_OpenFolder]
        public void Execute_OpenFolder(object obj = null)
        {
            try
            {
                if (MultiMode && SelectedDbs.Any())
                    SelectedDbs.Where(y => y.MDFLocation.Length > 0)
                      .Select(x => Path.GetDirectoryName(x.MDFLocation)).Distinct().ToList().ForEach(__RunOpenFolder);
                else if (SelectedDB != null && SelectedDB.MDFLocation != null &&
                  SelectedDB.MDFLocation.Length > 0)
                    __RunOpenFolder(Path.GetDirectoryName(SelectedDB.MDFLocation));

                Execute_Reload();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_CutLogFile]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_CutLogFile()
        {
            return __CanExecute_Common() && __IsLocal();
        }
        #endregion

        #region [CanExecute_OpenFolder]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_OpenFolder()
        {
            return __CanExecute_Common() && __IsLocal();
        }
        #endregion

        #region [CanExecute_OpenLastBackupFolder]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_OpenLastBackupFolder()
        {
            return __CanExecute_Common() && __IsLocal();
        }
        #endregion

        #region [Execute_OpenLastBackupFolder]
        public void Execute_OpenLastBackupFolder(object obj = null)
        {
            try
            {
                if (MultiMode && SelectedDbs.Any())
                    SelectedDbs.Where(y => y.LastBackupPath.Length > 0)
                      .Select(x => Path.GetDirectoryName(x.LastBackupPath)).Distinct().ToList().ForEach(__RunOpenFolder);
                else if (SelectedDB != null && SelectedDB.LastBackupPath != null &&
                  SelectedDB.LastBackupPath.Length > 0)
                    __RunOpenFolder(Path.GetDirectoryName(SelectedDB.LastBackupPath));
                Execute_Reload();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_DeleteUnusedFiles]
        [DependsUpon(nameof(UnusedFiles))]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_DeleteUnusedFiles() => __CanExecute_Common() && UnusedFiles.Any() && __IsLocal();
        #endregion

        #region [CanExecute_TakeAllOnline]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_TakeAllOnline() => __CanExecute_Common();
        #endregion

        #region [CanExecute_TakeAllOffline]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_TakeAllOffline() => __CanExecute_Common();
        #endregion

        #region [CanExecute_RestoreMultipleBaks]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_RestoreMultipleBaks() => Db != null;
        #endregion

        #region [CanExecute_Reload]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_Reload() => __CanExecute_Common();
        #endregion

        #region [Execute_RestoreMultipleBaks]
        public void Execute_RestoreMultipleBaks(object obj = null)
        {
            try
            {
                var FileDialog = new OpenFileDialog
                {
                    Filter = "bak files | *.bak",
                    Multiselect = true
                };
                FileDialog.ShowDialog(null);
                foreach (var item in FileDialog.FileNames)
                {
                    var Filename = Path.GetFileNameWithoutExtension(item);
                    __RestoreBackup(new List<DBStateEntry> { new DBStateEntry { DBName = Filename, LastBackupPath = item } }, true);
                }
                Execute_Reload();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [CanExecute_CloneDataBase]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_CloneDataBase() => __CanExecute_Common() && __IsLocal();
        #endregion

        #region [CanExecute_SwitchMultiMode]
        [DependsUpon(nameof(DBEntries))]
        [DependsUpon(nameof(ServerName))]
        public bool CanExecute_SwitchMultiMode() => __CanExecute_Common();
        #endregion

        #region [Execute_CloneDataBase]
        public void Execute_CloneDataBase(object obj = null)
        {
            try
            {
                if (MultiMode && SelectedDbs.Any())
                    __RunCloneDataBase(SelectedDbs);
                else if (SelectedDB != null)
                {
                    var Wnd = new InputBox("Choose Clone name", "Please type a name for the clone");
                    Wnd.OkRequested += __Clone_InputBox_OkRequested;
                    Wnd.ShowDialog();
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region [Execute_SwitchMultiMode]
        public void Execute_SwitchMultiMode(object obj = null)
        {
            try
            {
                __SetMultiMode(!MultiMode);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_CreateNewDatabase]
        public void Execute_CreateNewDatabase(object obj = null)
        {
            try
            {
                var Wnd = new InputBox("New database name", "Please enter your new database name");
                Wnd.OkRequested += __RunCreateNewDataBase;
                Wnd.ShowDialog();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_ShowSystemDatabases]
        public void Execute_ShowSystemDatabases(object obj = null)
        {
            try
            {
                m_ShowSystemDatabases = !m_ShowSystemDatabases;
                Execute_Reload();
                __WriteRegistryValue("ShowSystemDatabases", m_ShowSystemDatabases ? "1" : "0");
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_SwitchTracking]
        public void Execute_SwitchTracking(object obj = null)
        {
            try
            {
                m_FileTrackingEnabled = !m_FileTrackingEnabled;
                __ActivateTaskScheduler();
                Execute_Reload();
                __WriteRegistryValue("EnableFileSizeMonitoring", m_FileTrackingEnabled ? "1" : "0");
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_Reconnect]
        public void Execute_Reconnect(object obj = null)
        {
            try
            {
                m_db = null;
                skipReload = false;
                Execute_Reload();
            }
            catch (Exception ex)
            {
                __ResetInvalidConnection(this, new EventArgs());
            }
        }
        #endregion

        #region [Execute_StartMonitoring]
        public void Execute_StartMonitoring(object DatabaseEntry)
        {
            try
            {
                if (DatabaseEntry is DBStateEntry DBEntry)
                    m_TrackingDBStateEntry = DBEntry;
                if (m_TrackingDBStateEntry != null && m_TrackingDBStateEntry.TrackedFiles.Any())
                {
                    var DeleteTrackingResult = MessageBox.Show($"Are you sure to remove the File Monitoring for the {m_TrackingDBStateEntry.DBName} Database", "Remove File Monitoring", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (DeleteTrackingResult == MessageBoxResult.Yes)
                    {
                        __AssignTrackedFiles();
                        __RemoveTrackedFiles();
                    }
                    return;
                }
                var MaxFileSizeBox = new InputBox("Max FileSize", "Enter Max Filesize");
                MaxFileSizeBox.OkRequested += __MaxFileSizeBox_OkRequested;
                MaxFileSizeBox.ShowDialog();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region [Execute_SelectAll]
        public void Execute_SelectAll(object obj = null)
        {
            try
            {
                ListBoxDbs?.SelectAll();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_CutLogFile]
        public void Execute_CutLogFile(object obj = null)
        {
            try
            {
                if (SelectedDB is DBStateEntry State)
                    __RunCutLogFile(new List<DBStateEntry> { State });
                Execute_Reload();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [Execute_RenameDatabase]
        public void Execute_RenameDatabase(object obj = null)
        {
            try
            {
                if (SelectedDB is DBStateEntry State)
                    __RenameDataBase(new List<DBStateEntry> { State });
                Execute_Reload();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #endregion

        #region - private methods -

        #region [__GetODBCEntries]
        private string[] __GetODBCEntries() => Registry.LocalMachine.OpenSubKey(ODBC32BitRegPath).GetSubKeyNames();
        #endregion

        #region [__DeleteODBCEntries]
        private void __DeleteODBCEntries(List<DBStateEntry> Entries)
        {
            var Key = Registry.LocalMachine.OpenSubKey(ODBC32BitRegPath, true);
            foreach (var DBEntry in Entries)
            {
                m_DNSName = string.Format(m_DNSBase, ServerName, DBEntry.DBName);
                if (m_DNSName.Length > 32)
                    m_DNSName = m_DNSName.Substring(0, 32);
                if (Key.OpenSubKey(m_DNSName, true) != null)
                    Key.DeleteSubKey(m_DNSName);
            }
            Execute_Reload();
        }
        #endregion

        #region [__CreateODBCEntries]
        private void __CreateODBCEntries(List<DBStateEntry> Entries, bool Overwrite = false)
        {
            if (Overwrite)
                __DeleteODBCEntries(Entries);
            var Key = Registry.LocalMachine.OpenSubKey(ODBC32BitRegPath, true);
            foreach (var DBEntry in Entries)
            {
                m_DNSName = string.Format(m_DNSBase, ServerName, DBEntry.DBName);
                if (m_DNSName.Length > 32)
                    m_DNSName = m_DNSName.Substring(0, 32);
                if (Key.OpenSubKey(m_DNSName, true) != null && !Overwrite)
                    continue;
                var SubKey = Key.CreateSubKey(m_DNSName);
                SubKey.SetValue("Database", DBEntry.DBName);
                SubKey.SetValue("Driver", @"C:\WINDOWS\system32\SQLSRV32.dll");
                SubKey.SetValue("LastUser", m_User);
                SubKey.SetValue("Server", ServerName);
            }
            Execute_Reload();
        }
        #endregion

        #region [__MaxFileSizeBox_OkRequested]
        private void __MaxFileSizeBox_OkRequested(object sender, EventArgs e)
        {
            m_TrackingMaxFileSizeInput = sender.ToString();
            var FileSizeDimensionBox = new InputBox("File Size Dimension", "Please choose one of the following Dimensions: B, KB, MB, GB, TB, PB");
            FileSizeDimensionBox.OkRequested += __FileSizeDimensionBox_OkRequested;
            FileSizeDimensionBox.ShowDialog();
        }
        #endregion

        #region [__FileSizeDimensionBox_OkRequested]
        private void __FileSizeDimensionBox_OkRequested(object sender, EventArgs e)
        {
            eDataDimension CurrentDimension;
            switch (sender.ToStringValue())
            {
                default:
                case "B":
                    CurrentDimension = eDataDimension.Byte;
                    break;
                case "KB":
                    CurrentDimension = eDataDimension.Kilobyte;
                    break;
                case "MB":
                    CurrentDimension = eDataDimension.Megabyte;
                    break;
                case "GB":
                    CurrentDimension = eDataDimension.Gigabyte;
                    break;
                case "TB":
                    CurrentDimension = eDataDimension.Terabyte;
                    break;
                case "PB":
                    CurrentDimension = eDataDimension.Petabyte;
                    break;
            }
            m_TrackedFiles.Add(new Entry
            {
                FilePath = m_TrackingDBStateEntry.LDFLocation,
                MaxFileSize = m_TrackingMaxFileSizeInput.ToLongValue(),
                DataDimension = CurrentDimension
            });
            IO.WriteFileList(m_TrackedFiles);
            Execute_Reload();
        }
        #endregion

        #region [__AssignTrackedFiles]
        private void __AssignTrackedFiles()
        {
            m_TrackedFiles = IO.ReadFileList() ?? new List<Entry>();
            foreach (var TrackedFile in m_TrackedFiles)
            {
                var MatchingDBEntry = DBEntries.FirstOrDefault(x => x.LDFLocation.Equals(TrackedFile.FilePath) || x.MDFLocation.Equals(TrackedFile.FilePath));
                if (MatchingDBEntry != null)
                    MatchingDBEntry.TrackedFiles.Add(TrackedFile);
            }
        }
        #endregion

        #region [__AssignGeneralProps]
        private void __AssignGeneralProps()
        {
            var ODBCEntries = __GetODBCEntries();
            if (!DBEntries.Any())
                return;
            foreach (var DBEntry in DBEntries)
            {
                m_DNSName = string.Format(m_DNSBase, ServerName, DBEntry.DBName);
                if (m_DNSName.Length > 32)
                    m_DNSName = m_DNSName.Substring(0, 32);
                DBEntry.HasODBCEntry = ODBCEntries.Any(x => x.Equals(m_DNSName));
            }
        }
        #endregion

        #region [__RemoveTrackedFiles]
        private void __RemoveTrackedFiles()
        {
            if (m_TrackedFiles != null && m_TrackedFiles.Any())
            {
                m_TrackedFiles.Remove(m_TrackingDBStateEntry.TrackedFiles.FirstOrDefault());
                IO.WriteFileList(m_TrackedFiles);
            }
            Execute_Reload();
        }
        #endregion

        #region [GetRegistryValue]
        public static string GetRegistryValue(string key)
        {
            try
            {
                var BaseKey = Registry.CurrentUser.OpenSubKey("Software", true);
                var Key = BaseKey.OpenSubKey(nameof(DatabaseBuddy), true);
                if (Key == null)
                    Key = BaseKey.CreateSubKey(nameof(DatabaseBuddy), true);

                return Key.GetValue(key).ToStringValue();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        #endregion

        #region [__WriteRegistryValue]
        private static void __WriteRegistryValue(string key, string value)
        {
            if (value == null)
                return;
            var BaseKey = Registry.CurrentUser.OpenSubKey("Software", true);
            var Key = BaseKey.OpenSubKey(nameof(DatabaseBuddy), true);
            if (Key == null)
                Key = BaseKey.CreateSubKey(nameof(DatabaseBuddy), true);
            Key.SetValue(key, value);
        }
        #endregion

        #region [__OpenMSSQLQuery]
        private void __OpenMSSQLQuery(string DataBaseName)
        {
            if (m_MSSQLStudioPath.IsNullOrEmpty())
                return;

            using (Process ssms = new Process())
            {
                ssms.StartInfo.FileName = m_MSSQLStudioPath;
                ssms.StartInfo.Arguments =
                  $"-nosplash " +
                  $" -S {ServerName} " +
                  $" -d {DataBaseName}" +
                  $" -U {m_User}" +
                  $" -P {m_Password}";
                ssms.Start();
            }
        }
        #endregion

        #region [__GetMSSQLStudioPath]
        private void __GetMSSQLStudioPath(object obj = null)
        {
            try
            {
                var ExeName = "Ssms.exe";
                foreach (string folder in Environment.GetEnvironmentVariable("path").Split(';'))
                {
                    if (File.Exists(folder + ExeName))
                    {
                        m_MSSQLStudioPath = folder + ExeName;
                        break;
                    }
                    else if (File.Exists(folder + "\\" + ExeName))
                    {
                        m_MSSQLStudioPath = folder + "\\" + ExeName;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [__CanExecute_Common]
        private bool __CanExecute_Common() => DBEntries.Any();
        #endregion

        #region [__IsLocal]
        private bool __IsLocal() => ServerName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) || ServerName.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase);
        #endregion

        #region [__SetMultiMode]
        private void __SetMultiMode(bool MultiAllowed)
        {
            MultiMode = MultiAllowed;
            OnPropertyChanged(nameof(MultiMode));
            DBEntries.ForEach(x => x.ForMultiMode = false);
        }
        #endregion

        #region [__ReloadDBs]
        private void __ReloadDBs()
        {
            DBEntries = null;
            _ = __LoadDataBases(eDATABASESTATE.ONLINE);
            _ = __LoadDataBases(eDATABASESTATE.OFFLINE);
        }
        #endregion

        #region [__LoadDataBases]
        private List<DBStateEntry> __LoadDataBases(eDATABASESTATE dbstate)
        {
            try
            {
                SqlDataReader tmpReader = null;
                if (Db != null)
                {
                    var Cmd = "SELECT name FROM sys.databases ";
                    if (dbstate == eDATABASESTATE.ONLINE)
                        Cmd += "WHERE state_desc = 'ONLINE' ";
                    if (dbstate == eDATABASESTATE.OFFLINE)
                        Cmd += "WHERE state_desc = 'OFFLINE' ";
                    Cmd += ";";
                    var tmpDatabaseEntries = new List<DBStateEntry>();
                    using (var Reader = Db.GetDataReader(Cmd))
                    {
                        if (Reader == null)
                            return tmpDatabaseEntries;
                        if (Reader.HasRows)
                        {
                            tmpReader = Reader;
                            while (Reader.Read())
                            {
                                var dbName = Reader["name"].ToStringValue();

                                if ((!m_ShowSystemDatabases && m_systemDataBases.Any(x => x.Equals(dbName)))
                                  || DBEntries.Any(x => x.DBName.Equals(dbName)))
                                    continue;

                                tmpDatabaseEntries.Add(new DBStateEntry
                                {
                                    DBName = Reader["name"].ToStringValue(),
                                    IsOnline = dbstate == eDATABASESTATE.ONLINE,
                                });
                            }
                            Reader.Close();
                            Db.CloseDataReader();
                        }
                    }
                    tmpDatabaseEntries.AddRange(DBEntries);
                    foreach (var DBEntry in tmpDatabaseEntries)
                    {
                        __LoadDatabaseFilePaths(DBEntry);
                        __GetLastBackupData(DBEntry);
                    }
                    DBEntries = tmpDatabaseEntries;
                    __WriteRegistryValue(nameof(ServerName), ServerName);
                    return tmpDatabaseEntries;
                }
                else
                {

                }
                return new List<DBStateEntry>();
            }
            catch (Exception ex)
            {
                return new List<DBStateEntry>();
            }

        }
        #endregion

        #region [__LoadDatabaseFilePaths]
        private void __LoadDatabaseFilePaths(DBStateEntry DBEntry)
        {
            if (Db != null)
            {
                Db.AddParameter("@DataBaseName", DBEntry.DBName);
                var PathReader = Db.GetDataReader(Statements.GETLASTBACKUPDATA);
                if (PathReader.HasRows)
                {
                    while (PathReader.Read())
                    {
                        if (PathReader["type_desc"].ToStringValue().Equals("LOG"))
                        {
                            DBEntry.LDFLocation = PathReader["physical_name"].ToStringValue();
                            DBEntry.LDFSize = PathReader[nameof(Size)].ToInt32Value();
                        }
                        else
                        {
                            DBEntry.MDFLocation = PathReader["physical_name"].ToStringValue();
                            DBEntry.MDFSize = PathReader[nameof(Size)].ToInt32Value();
                        }
                    }
                    PathReader.Close();
                    Db.CloseDataReader();
                }
            }
        }
        #endregion

        #region [__KillConnections]
        private void __KillConnections(List<DBStateEntry> DataBaseEntries)
        {
            if (DataBaseEntries == null)
                return;

            if (Db != null)
            {
                foreach (var DataBaseName in DataBaseEntries)
                {
                    if (DataBaseName.DBName.Equals("master", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    var IdsToKill = new List<int>();
                    using (var Reader = Db.GetDataReader(Statements.GETACTIVECONNECTIONS))
                    {
                        if (Reader == null)
                            return;
                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                var IdToKill = int.MaxValue;
                                _ = int.TryParse(Reader["ID"].ToStringValue(), out IdToKill);
                                IdsToKill.Add(IdToKill);
                            }
                        }
                        Reader.Close();
                        Db.CloseDataReader();
                    }

                    var Cmd = "USE[master] \n";
                    var builder = new System.Text.StringBuilder();
                    builder.Append(Cmd);
                    foreach (var Id in IdsToKill)
                    {
                        builder.Append($"KILL {Id}; ");
                    }
                    Cmd = builder.ToString();
                    Cmd += $" ALTER DATABASE [{DataBaseName.DBName}] SET OFFLINE ";
                    try
                    {
                        Db.ExecuteNonQuery(Cmd);
                    }
                    catch (Exception)
                    {
                        //Default Exception Process to kill the Connections is also a Process and cannot kill itself
                        continue;
                    }
                }
            }
            Execute_Reload();
        }
        #endregion

        #region [__ActivateConnections]
        private void __ActivateConnections(List<DBStateEntry> DataBaseEntries)
        {
            if (DataBaseEntries == null || !DataBaseEntries.Any())
                return;

            if (Db != null)
            {
                foreach (var DataBaseName in DataBaseEntries)
                {
                    if (DataBaseName.IsSystemDataBase)
                        continue;
                    var Cmd = "USE[master] \n";
                    Cmd += $" ALTER DATABASE [{DataBaseName.DBName}] SET ONLINE ";
                    Db.AddParameter("@DataBaseName", DataBaseName.DBName);
                    Db.ExecuteNonQuery(Cmd);
                }
            }
            Execute_Reload();
        }
        #endregion

        #region [__DeleteDataBase]
        private void __DeleteDataBase(List<DBStateEntry> DataBaseEntries, bool silentDelete = false, bool deleteDatabaseFiles = true)
        {
            if (DataBaseEntries == null || !DataBaseEntries.Any())
                return;
            var Messagetext = $"Are you sure to delete the following databases?\n";
            DataBaseEntries.Where(x => !x.IsSystemDataBase).ToList().ForEach(x => Messagetext += $"-{x.DBName}\n");
            if (!silentDelete)
            {
                var DeleteResult = MessageBox.Show(Messagetext, "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (DeleteResult == MessageBoxResult.No)
                    return;
            }
            __KillConnections(DataBaseEntries);

            if (Db != null)
            {
                foreach (var DataBaseName in DataBaseEntries.Where(x => !x.IsSystemDataBase))
                {
                    var Cmd = "USE[master] \n";
                    Cmd += $" DROP DATABASE [{DataBaseName.DBName}]";
                    Db.ExecuteNonQuery(Cmd);
                    if (deleteDatabaseFiles)
                    {
                        File.Delete(DataBaseName.MDFLocation);
                        File.Delete(DataBaseName.LDFLocation);
                    }
                }
            }
            Execute_Reload();
        }
        #endregion

        #region [__GetLastBackupData]
        private void __GetLastBackupData(DBStateEntry DataBaseEntry)
        {
            var LastBackupPath = string.Empty;
            var LastBackupTime = string.Empty;

            if (Db != null)
            {
                Db.AddParameter("@DataBaseName", DataBaseEntry.DBName);

                using (SqlDataReader Reader = Db.GetDataReader(Statements.GETLASTBACKUP))
                {
                    if (Reader != null)
                    {
                        if (Reader.HasRows)
                        {
                            while (Reader.Read())
                            {
                                LastBackupPath = Reader["BackupFileLocation"].ToStringValue();
                                if (!Reader.IsDBNull(2))
                                    LastBackupTime = $"{Reader["LastFullDBBackupDate"].ToDateTimeValue().ToShortDateString()} {Reader["LastFullDBBackupDate"].ToDateTimeValue().ToShortTimeString()}";
                            }
                        }
                        Reader.Close();
                    }
                    Db.CloseDataReader();
                }
                DataBaseEntry.LastBackupTime = string.Empty;
                DataBaseEntry.LastBackupPath = string.Empty;
                if (File.Exists(LastBackupPath))
                {
                    DataBaseEntry.LastBackupTime = LastBackupTime;
                    DataBaseEntry.LastBackupPath = LastBackupPath;
                }
                else
                {
                    DataBaseEntry.LastBackupPath = string.Empty;
                    if (LastBackupTime.Length > 0)
                        DataBaseEntry.LastBackupTime = $"Backup from {LastBackupTime} not found";
                }
            }
        }
        #endregion

        #region [__RestoreBackup]
        private void __RestoreBackup(List<DBStateEntry> DataBaseEntries, bool SilentRestore = false)
        {
            try
            {
                if (DataBaseEntries == null || !DataBaseEntries.Any())
                    return;
                var Messagetext = $"Are you sure to restore the following databases?\n";
                DataBaseEntries.Where(x => !x.IsSystemDataBase).ToList().ForEach(x => Messagetext += $"-{x.DBName} Last Backup {x.LastBackupTime}\n");
                if (!SilentRestore)
                {
                    var RestoreBackupResult = MessageBox.Show(Messagetext, "Confirm restore", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (RestoreBackupResult == MessageBoxResult.No)
                        return;
                }
                __KillConnections(DataBaseEntries);

                if (Db != null)
                {
                    foreach (var DataBase in DataBaseEntries)
                    {
                        if (DataBase.LastBackupPath.Length == 0)
                            continue;
                        var builder = new System.Text.StringBuilder();
                        var Cmd = "USE[master] \n";
                        _ = builder.Append(Cmd);

                        builder.Append($@"RESTORE DATABASE [{DataBase.DBName}]
 FROM DISK = N'{DataBase.LastBackupPath}' WITH REPLACE;");
                        Cmd = builder.ToString();
                        _ = Db.ExecuteNonQuery(Cmd); ;
                    }
                }
                Execute_Reload();
                MessageBox.Show($"Erfolgreich wiederhergestellt", "Wiederherstellung erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                __ActivateConnections(DataBaseEntries);
            }
        }
        #endregion

        #region [__BackupDatabases]
        private void __BackupDatabases(List<DBStateEntry> Entries)
        {
            __ActivateConnections(Entries);
            var BackupTime = DateTime.Now.ToString("ddMMyyyy HHmm");
            var CmdBackup = "USE [master];\n";
            var builder = new System.Text.StringBuilder();
            _ = builder.Append(CmdBackup);
            if (Db != null)
            {
                foreach (var item in Entries)
                {
                    if (item.DBName.Equals("master", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    var BackupPath = Path.GetDirectoryName(item.MDFLocation) + $@"\backup\{BackupTime}";
                    _ = Directory.CreateDirectory(BackupPath);
                    _ = builder.Append($@"BACKUP DATABASE [{item.DBName}]
TO DISK = '{BackupPath}\{item.DBName}.bak';");
                }
                CmdBackup = builder.ToString();
                _ = Db.ExecuteNonQuery(CmdBackup);
            }
            Execute_Reload();
            _ = MessageBox.Show("Datenbankbackups erfolgreich erzeugt", "Backup erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region [__GetUnusedDataBaseFiles]
        private void __GetUnusedDataBaseFiles()
        {
            try
            {
                UnusedFiles = new List<string>();
                if (Db != null)
                {
                    foreach (var item in DBEntries)
                    {
                        var AllDBFilePaths = new List<string>();
                        const string GetFilesCmd = "\tUSE [master];\r\nSELECT\r\n  physical_name 'FileLocation'\r\nFROM sys.master_files;";
                        using (var DataReader = Db.GetDataReader(GetFilesCmd))
                        {
                            if (DataReader.HasRows)
                                while (DataReader.Read())
                                    AllDBFilePaths.Add(DataReader["FileLocation"].ToStringValue());
                            DataReader?.Close();
                            Db.CloseDataReader();
                        }
                        if (item.MDFLocation == null || item.MDFLocation.Length == 0)
                            continue;
                        var FilesToDelete = Directory.GetFiles(Path.GetDirectoryName(item.MDFLocation), "*.*").Where(x => !AllDBFilePaths.Contains(x))
                    .Where(file => file.ToLower().EndsWith("mdf") || file.ToLower().EndsWith("ldf"))
                    .ToList();
                        UnusedFiles.AddRange(FilesToDelete);

                    }
                }
                UnusedFiles = UnusedFiles.Distinct().ToList();
                OnPropertyChanged(nameof(UnusedFiles));
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region [__RunOpenFolder]
        private static void __RunOpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                _ = System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                _ = MessageBox.Show($"{folderPath} Directory does not exist!");
            }
        }
        #endregion

        #region [__RunCutLogFile]
        private void __RunCutLogFile(List<DBStateEntry> Entries)
        {
            //Disable Not Working yet
            return;
            var tmpGuid = Guid.NewGuid();
            __KillConnections(Entries);
            if (Db != null)
            {
                foreach (var Entry in Entries)
                {
                    var OldName = Path.GetFileName(Entry.MDFLocation);
                    var NewName = tmpGuid + OldName;
                    File.Copy(Entry.LDFLocation, $@"{Path.GetDirectoryName(Entry.MDFLocation)}\{NewName}");
                    File.Delete(Entry.MDFLocation);
                    File.Delete(Entry.LDFLocation);
                    File.Move($@"{Path.GetDirectoryName(Entry.MDFLocation)}\{NewName}", $@"{Path.GetDirectoryName(Entry.MDFLocation)}\{OldName}");
                    __DeleteDataBase(new List<DBStateEntry> { Entry }, true, false);
                    var Cmd = $@"CREATE DATABASE [{Entry.DBName}] 
    ON (FILENAME = '{Entry.MDFLocation}')
    FOR ATTACH;";
                    Db.ExecuteNonQuery(Cmd);
                }
            }
            Execute_Reload();
        }
        #endregion

        #region [__RenameDataBase]
        private void __RenameDataBase(List<DBStateEntry> Entries)
        {
            var Wnd = new InputBox("Choose the new name", "Please type the new name", Entries.First().DBName);
            Wnd.OkRequested += __NewNameTyped;
            Wnd.ShowDialog();
        }
        #endregion

        #region [__NewNameTyped]
        private void __NewNameTyped(object sender, EventArgs e)
        {
            if (SelectedDB is DBStateEntry State && Db != null)
            {
                if (DBEntries.Any(x => x.DBName.Equals(sender.ToString())))
                {
                    var Wnd = new InputBox("Rename failed", $"Database Name {sender} already exists. Please try again", State.DBName);
                    Wnd.OkRequested += __NewNameTyped;
                    Wnd.ShowDialog();
                }
                else
                {
                    Db.ExecuteNonQuery($@"USE [MASTER] ALTER DATABASE [{State.DBName}] MODIFY NAME = [{sender}] ;");
                    Execute_Reload();
                }
            }
        }
        #endregion

        #region [__CutAllLogFiles]
        private void __CutAllLogFiles() => __RunCutLogFile(DBEntries);
        #endregion

        #region [__RunCloneDataBase]
        private void __RunCloneDataBase(List<DBStateEntry> Entries)
        {
            try
            {
                if (!Entries.Any())
                    return;
                __KillConnections(Entries);

                if (Db != null)
                {
                    foreach (var Entry in Entries)
                    {
                        if (Entry.CloneName == null || Entry.CloneName.Length == 0)
                            Entry.CloneName = @$"{Entry.DBName}_Clone";
                        var NewMDFLocation = @$"{Path.GetDirectoryName(Entry.MDFLocation)}\{Entry.CloneName}.mdf";
                        var NewLDFLocation = @$"{Path.GetDirectoryName(Entry.LDFLocation)}\{Entry.CloneName}.ldf";
                        if (DBEntries.Any(x => x.DBName.Equals(Entry.CloneName)))
                            continue;
                        File.Copy(Entry.MDFLocation, NewMDFLocation, true);
                        File.Copy(Entry.LDFLocation, NewLDFLocation, true);
                        var Cmd = $@"USE [master];
CREATE DATABASE [{Entry.CloneName}]
    ON (FILENAME = '{NewMDFLocation}'),
       (FILENAME = '{NewLDFLocation}')
    FOR ATTACH;";
                        _ = Db.ExecuteNonQuery(Cmd);
                    }
                }
                __ActivateConnections(Entries);
                Execute_Reload();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [__RunCreateNewDataBase]
        private void __RunCreateNewDataBase(object sender, EventArgs e)
        {
            if (sender == null || ((string)sender).Length == 0)
                return;

            if (Db != null)
            {
                var Cmd = "USE[master] \n";
                Cmd += $" CREATE DATABASE [{sender as string}];";
                Db.ExecuteNonQuery(Cmd);
            }
            Execute_Reload();
        }
        #endregion

        #region [__Clone_InputBox_OkRequested]
        private void __Clone_InputBox_OkRequested(object sender, EventArgs e)
        {
            SelectedDB.CloneName = sender as string;
            __RunCloneDataBase(new List<DBStateEntry> { SelectedDB });
            _ = MessageBox.Show($"Successful cloned '{SelectedDB.DBName}' to {SelectedDB.CloneName}");
        }
        #endregion

        #region [__ActivateTaskScheduler]
        private void __ActivateTaskScheduler()
        {
            if (m_ScheduleActivated || !m_FileTrackingEnabled)
                return;
            var response = WindowTaskScheduler.Configure()
                                              .CreateTask("FileMonitoring", @"C:\Windows\System32\WindowsPowerShell\v1.0\powerShell.exe command " + $"\"{Constants.APPLICATION_DOCUMENTS_FOLDER}\\Execute.ps1\"")
                                              .RunDaily()
                                              .RunEveryXMinutes(60)
                                              .RunDurationFor(new TimeSpan(8, 30, 0))
                                              .SetStartDate(DateTime.Now.AddDays(1))
                                              .SetStartTime(new TimeSpan(8, 0, 0))
                                              .Execute();
            if (response.IsSuccess)
                __WriteRegistryValue("EnabledSchedule", "1");
        }
        #endregion

        #region [__HandleFileTrackingNeeds]
        private void __HandleFileTrackingNeeds()
        {
            try
            {
                if (!Directory.Exists(Constants.APPLICATION_DOCUMENTS_FOLDER))
                    Directory.CreateDirectory(Constants.APPLICATION_DOCUMENTS_FOLDER);

                new List<string>
      {
        "Newtonsoft.Json.dll",
        "Övervakning.Executeable.dll",
        "Övervakning.Shared.dll",
        "Övervakning.Executeable.deps.json",
        "Övervakning.Executeable.runtimeconfig.json",
      }.ForEach(x => __CopyFileIfNotExist(x));

                if (!File.Exists(Path.Combine(Constants.APPLICATION_DOCUMENTS_FOLDER, "Execute.ps1")))
                {
                    File.Create(Path.Combine(Constants.APPLICATION_DOCUMENTS_FOLDER, "Execute.ps1")).Close();
                    File.WriteAllText(Path.Combine(Constants.APPLICATION_DOCUMENTS_FOLDER, "Execute.ps1"), $"Set-ExecutionPolicy remotesigned -force \n $mydocuments = [environment]::getfolderpath(\"mydocuments\") \n" +
                      $"cd $mydocuments \n cd \"Övervakning\" \ndotnet Övervakning.Executeable.dll --NoVisualAlert", Encoding.UTF8);
                }
                __ActivateTaskScheduler();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [__CopyFileIfNotExist]
        private static void __CopyFileIfNotExist(string FileName, bool OverWrite = false)
        {
            return;
            if (!File.Exists(Path.Combine(Constants.APPLICATION_DOCUMENTS_FOLDER, FileName)))
                File.Copy(@$"\\DatabaseBuddy\RequiredDLLs\{FileName}", Path.Combine(Constants.APPLICATION_DOCUMENTS_FOLDER, FileName), OverWrite);
        }
        #endregion

        #region [__GetVersion]
        private static string __GetVersion()
        {
            return "v " + Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
        #endregion

        #region [__InitializeCommands]
        private void __InitializeCommands()
        {
            Reload = new DelegateCommand<object>(Execute_Reload);
            CreateNewDatabase = new DelegateCommand<object>(Execute_CreateNewDatabase);
            ShowSystemDatabases = new DelegateCommand<object>(Execute_ShowSystemDatabases);
            SwitchMultiMode = new DelegateCommand<object>(Execute_SwitchMultiMode);
            SelectAll = new DelegateCommand<object>(Execute_SelectAll);
            SwitchTracking = new DelegateCommand<object>(Execute_SwitchTracking);
            Reconnect = new DelegateCommand<object>(Execute_Reconnect);
            GotLbFocus = new DelegateCommand<object>(Execute_GotLbFocus);
            DeleteSelectedDataBase = new DelegateCommand<object>(Execute_DeleteSelectedDataBase);
            OpenFolder = new DelegateCommand<object>(Execute_OpenFolder);
            OpenLastBackupFolder = new DelegateCommand<object>(Execute_OpenLastBackupFolder);
            CloneDataBase = new DelegateCommand<object>(Execute_CloneDataBase);
            RenameDatabase = new DelegateCommand<object>(Execute_RenameDatabase);
            CutLogFile = new DelegateCommand<object>(Execute_CutLogFile);
            OpenQuery = new DelegateCommand<object>(Execute_OpenQuery);
            GenerateODBCEntry = new DelegateCommand<object>(Execute_GenerateODBCEntry);
            TakeSelectedOffline = new DelegateCommand<object>(Execute_TakeSelectedOffline);
            BackupSelectedDataBase = new DelegateCommand<object>(Execute_BackupSelectedDataBase);
            RestoreBackup = new DelegateCommand<object>(Execute_RestoreBackup);
            StartMonitoring = new DelegateCommand<object>(Execute_StartMonitoring);
            TakeAllOffline = new DelegateCommand<object>(Execute_TakeAllOffline);
            TakeAllOnline = new DelegateCommand<object>(Execute_TakeAllOnline);
            BackupAll = new DelegateCommand<object>(Execute_BackupAll);
            RestoreAll = new DelegateCommand<object>(Execute_RestoreAll);
            DeleteUnusedFiles = new DelegateCommand<object>(Execute_DeleteUnusedFiles);
            RestoreMultipleBaks = new DelegateCommand<object>(Execute_RestoreMultipleBaks);
        }
        #endregion

        #endregion
    }
}
