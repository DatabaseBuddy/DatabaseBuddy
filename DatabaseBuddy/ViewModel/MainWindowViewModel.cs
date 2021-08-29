using ControlzEx.Theming;
using DatabaseBuddy.Core;
using DatabaseBuddy.Core.Attributes;
using DatabaseBuddy.Core.DatabaseExtender;
using DatabaseBuddy.Core.Extender;
using DatabaseBuddy.Entities;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DatabaseBuddy.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region [needs]
        private List<string> m_systemDataBases;
        private List<string> m_UnusedFiles;
        private string m_Server = "localhost";
        private string m_UserName;
        private string m_Password;
        private const string ODBC32BitRegPath = @"SOFTWARE\WOW6432Node\ODBC\ODBC.INI";
        private string m_MSSQLStudioPath;
        private DBConnection m_db;
        private string m_DNSName = string.Empty;
        private string m_DNSBase = "{0}_{1}";

        private string m_DefaultDataPath;
        private string m_InstanceName;

        private bool m_ShowSystemDatabases;
        private bool m_FileTrackingEnabled;
        private bool m_ScheduleActivated;
        private bool skipReload;
        private DBStateEntry m_SelectedDB;
        private List<DBStateEntry> m_DBEntries;
        private ListBox m_ListBoxDbs;
        private bool m_IsBusy;
        private bool m_MultiMode;
        private bool m_SettingsOpen;
        private bool m_IntegratedSecurity;
        private string m_BaseTheme = "Light";
        private string m_SelectedTheme = "Blue";
        private MetroWindow MetroWnd;
        private string m_DBFilter;
        private static long m_MaxLogSize;
        private double m_ScalingValue = 1;
        private static int m_MaxBackupCount;
        private bool m_AutoCleanBackups;
        #endregion

        #region [Ctor]
        public MainWindowViewModel()
        {
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");

            if (Application.Current.MainWindow is MetroWindow tmpMetroWindow)
            {
                MetroWnd = tmpMetroWindow;
            }
            __GetRegistryValues();
            __InitializeCommands();
            __GetExtendedDBInformations();
            m_systemDataBases = new List<string> { "master", "tempdb", "model", "msdb" };
            UnusedFiles = new List<string>();
            if (m_MSSQLStudioPath.IsNullOrEmpty())
                __GetMSSQLStudioPath();

            __HandleTheming();
            skipReload = false;
            Execute_Reload();
        }

        #endregion

        #region [Commands]
        public ICommand Reload { get; set; }
        public ICommand CreateNewDatabase { get; set; }
        public ICommand ShowSystemDatabases { get; set; }
        public ICommand SwitchMultiMode { get; set; }
        public ICommand SelectAll { get; set; }
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
        public ICommand ToggleSettings { get; set; }
        public ICommand ChangeBaseTheme { get; set; }
        public ICommand ChangeTheme { get; set; }
        public ICommand RestartSQLServerInstance { get; set; }
        public ICommand RestartAsAdmin { get; set; }
        public ICommand IncreaseScaling { get; set; }
        public ICommand DecreaseScaling { get; set; }
        public ICommand ResetScaling { get; set; }

        #endregion

        #region - properties -

        #region - public properties -

        public string DBFilter
        {
            get
            {
                return m_DBFilter;
            }
            set
            {
                m_DBFilter = value;
                ListBoxDbs.ItemsSource = null;
                if (value.IsNullOrEmpty())
                    ListBoxDbs.ItemsSource = DBEntries;
                else
                    ListBoxDbs.ItemsSource = DBEntries.Where(x => x.DBName.Contains(value.ToString(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public bool FileTrackingEnabled
        {
            get
            {
                return m_FileTrackingEnabled;
            }
            set
            {
                m_FileTrackingEnabled = value;
                OnPropertyChanged(nameof(FileMonitoringVisibility));
            }
        }

        [DependsUpon(nameof(ServerName))]
        public string SystemInformation
        {
            get
            {
                if (!IntegratedSecurity)
                    return $"{__GetVersion()} Server: {m_Server} User: {m_UserName}";
                else
                    return $"{__GetVersion()} Server: {m_Server} Integrated Security";
            }
        }

        public bool BaseThemeToggled
        {
            get
            {
                if (m_BaseTheme.IsNullOrEmpty())
                    return false;
                return !m_BaseTheme.Equals(ThemeManager.BaseColorLight);
            }
        }

        public bool SystemDataBasesToggled
        {
            get
            {
                return m_ShowSystemDatabases;
            }
        }

        public bool SettingsOpen
        {
            get
            {
                return m_SettingsOpen;
            }
            set
            {
                m_SettingsOpen = value;
                OnPropertyChanged(nameof(SettingsOpen));
            }
        }

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
                OnPropertyChanged(nameof(ListBoxSelectionMode));
                OnPropertyChanged(nameof(MultiModeToggleCaption));
                OnPropertyChanged(nameof(SelectAllVisibility));
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
                OnPropertyChanged(nameof(DeleteUnusedFilesCaption));
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
        public bool RestrictedRights => !SelectedDB?.IsSystemDatabase ?? false;

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

        public double AllDBSize => Math.Round(DBEntries.Select(x => x.DataBaseSize).Sum().ToByte().ToGigaByte(), 2);
        public double AllBackupSize => Math.Round(DBEntries.Select(x => x.AllBackupSize).Sum().ToByte().ToGigaByte(), 2);
        public double AllSize => Math.Round(AllDBSize + AllBackupSize, 2);

        [DependsUpon(nameof(UnusedFiles))]
        public string DeleteUnusedFilesCaption => !UnusedFiles.Any() ? "No unused database files" : $"Delete {UnusedFiles.Count} unused database files";

        [DependsUpon(nameof(MultiMode))]
        public SelectionMode ListBoxSelectionMode => MultiMode ? SelectionMode.Multiple : SelectionMode.Single;

        [DependsUpon(nameof(MultiMode))]
        public Visibility SelectAllVisibility => MultiMode ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
      ? Visibility.Collapsed : Visibility.Visible;

        [DependsUpon(nameof(m_FileTrackingEnabled))]
        public Visibility FileMonitoringVisibility => FileTrackingEnabled ? Visibility.Visible : Visibility.Collapsed;

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
                        if (UserName.IsNullOrEmpty() || m_Password.IsNullOrEmpty())
                            IntegratedSecurity = true;
                        if (IntegratedSecurity)
                            m_db = new DBConnection(ServerName, "master", true);
                        else
                            m_db = new DBConnection(ServerName, "master", m_UserName, m_Password);
                        m_db.ConnectionFailed += __ResetInvalidConnection;
                    }
                }
                catch (Exception ex)
                {
                    __ThrowMessage($"{nameof(Db)} failed!", ex.ToString());
                }
                return m_db;
            }
        }

        public Visibility CredentialsVisibility
        {
            get
            {
                return IntegratedSecurity ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool IntegratedSecurity
        {
            get
            {
                return m_IntegratedSecurity;
            }
            set
            {
                m_IntegratedSecurity = value;
                OnPropertyChanged(nameof(IntegratedSecurity));
                __WriteRegistryValue(nameof(IntegratedSecurity), value ? "1" : "0");
                OnPropertyChanged(nameof(CredentialsVisibility));
                OnPropertyChanged(nameof(SystemInformation));
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
                m_Server = value?.Trim() ?? "localhost";
                OnPropertyChanged(nameof(ServerName));
                OnPropertyChanged(nameof(SystemInformation));
            }
        }

        public string UserName
        {
            get
            {
                return m_UserName;
            }
            set
            {
                m_UserName = value;
                OnPropertyChanged(nameof(UserName));
                OnPropertyChanged(nameof(SystemInformation));
            }
        }

        public string Password
        {
            get
            {
                return m_Password;
            }
            set
            {
                m_Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ObservableCollection<string> Themes
        {
            get
            {
                return __GetThemeCboItems();
            }
        }

        public string SelectedTheme
        {
            get
            {
                return m_SelectedTheme;
            }
            set
            {
                if (value.IsNullOrEmpty())
                    return;
                m_SelectedTheme = value;
                Execute_ChangeTheme();
                OnPropertyChanged(nameof(SelectedTheme));
            }
        }

        public static long MaxLogSize
        {
            get
            {
                return m_MaxLogSize;
            }
            set
            {
                m_MaxLogSize = value;
                __WriteRegistryValue(nameof(MaxLogSize), m_MaxLogSize.ToString());
            }
        }

        public int MaxBackupCount
        {
            get
            {
                return m_MaxBackupCount;
            }
            set
            {
                m_MaxBackupCount = value;
                __WriteRegistryValue(nameof(MaxBackupCount), value.ToString());
            }
        }

        public double ScalingValue
        {
            get
            {
                return m_ScalingValue;
            }
            set
            {
                if (value <= 0.1)
                    return;
                m_ScalingValue = value;
                __WriteRegistryValue(nameof(ScalingValue), value.ToString());
                MetroWnd.LayoutTransform = new ScaleTransform(value, value);
                MetroWnd.Title = value == 1 ? $"{nameof(DatabaseBuddy)}" : $"{nameof(DatabaseBuddy)} {(value * 100).ToInt32Value()}%";
                OnPropertyChanged(nameof(ScalingValue));
            }
        }

        public bool AutoCleanBackups
        {
            get
            {
                return m_AutoCleanBackups;
            }
            set
            {
                m_AutoCleanBackups = value;
                OnPropertyChanged(nameof(AutoCleanBackups));
                __WriteRegistryValue(nameof(AutoCleanBackups), value ? "1" : "0");
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
                        var KillResult = DialogManager.ShowModalMessageExternal(MetroWnd, "Confirm taking offline", Messagetext, MessageDialogStyle.AffirmativeAndNegative/*, MessageSettings*/);
                        if (KillResult == MessageDialogResult.Canceled || KillResult == MessageDialogResult.Negative)
                            return;
                        else if (KillResult == MessageDialogResult.Affirmative)
                            __KillConnections(DataBaseEntries);
                    }
                    else
                        __ActivateConnections(new List<DBStateEntry> { State });
                }
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_TakeSelectedOffline)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_OpenQuery)} failed!", ex.ToString());
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
                    if (State.HandleODBCState.Equals("Delete ODBC Entry"))
                        __DeleteODBCEntries(new List<DBStateEntry> { State });
                    else
                        __CreateODBCEntries(new List<DBStateEntry> { State });
                }
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_GenerateODBCEntry)} failed!", ex.ToString());
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
                DataBaseEntries.Where(x => !x.IsSystemDatabase).ToList().ForEach(x => Messagetext += $"-{x.DBName}\n");
                var KillResult = DialogManager.ShowModalMessageExternal(MetroWnd, "Confirm taking offline", Messagetext, MessageDialogStyle.AffirmativeAndNegative);
                if (KillResult == MessageDialogResult.Canceled || KillResult == MessageDialogResult.Negative)
                    return;
                __KillConnections(DataBaseEntries.Where(x => !x.IsSystemDatabase).ToList());
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_TakeAllOffline)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_TakeAllOnline)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_DeleteSelectedDataBase)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_GotFocus)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_GotLbFocus)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_BackupAll)} failed!", ex.ToString());
            }
            finally
            {
                Execute_Reload();
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
                __ThrowMessage($"{nameof(Execute_RestoreBackup)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_BackupSelectedDataBase)} failed!", ex.ToString());
            }
            finally
            {
                Execute_Reload();
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
                __ThrowMessage($"{nameof(Execute_RestoreAll)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [Execute_Reload]
        public void Execute_Reload(object obj = null)
        {
            try
            {
                if (skipReload && obj == null)
                    return;
                Db.ExecuteScalar("USE [master] SELECT TOP (1) xserver_name FROM [master].[dbo].[spt_fallback_db]");
                __ReloadDBs();
                DBEntries.ForEach(x => x.AllBackups = x.GetBackups());
                __GetUnusedDataBaseFiles();
                __SetMultiMode(false);
                __AssignGeneralProps();
                if (ListBoxDbs != null)
                {
                    ListBoxDbs.ItemsSource = null;
                    ListBoxDbs.ItemsSource = DBEntries;
                    var tmpFilterValue = DBFilter;
                    DBFilter = tmpFilterValue;
                }
                __FireChangedEvents();
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_Reload)} failed!", ex.ToString());
                __ResetInvalidConnection(this, new EventArgs());
            }
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
                var DeleteResult = DialogManager.ShowModalMessageExternal(MetroWnd, "Confirm delete", Messagetext, MessageDialogStyle.AffirmativeAndNegative/*, MessageSettings*/);
                if (DeleteResult == MessageDialogResult.Canceled || DeleteResult == MessageDialogResult.Negative)
                    return;
                else
                {
                    UnusedFiles.ForEach(File.Delete);
                    Execute_Reload();
                }
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_DeleteUnusedFiles)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_OpenFolder)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_OpenLastBackupFolder)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_RestoreMultipleBaks)} failed!", ex.ToString());
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
                    __RunCloneDatabaseWithAnyName(new List<DBStateEntry> { SelectedDB });
                }
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_CloneDataBase)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_SwitchMultiMode)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [Execute_CreateNewDatabase]
        public void Execute_CreateNewDatabase(object obj = null)
        {
            try
            {
                __RunCreateNewDataBase();
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_CreateNewDatabase)} failed!", ex.ToString());
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
                __WriteRegistryValue(nameof(ShowSystemDatabases), m_ShowSystemDatabases ? "1" : "0");
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_ShowSystemDatabases)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [CanExecute_Reconnect]
        public bool CanExecute_Reconnect()
        {
            return __CanExecute_Common() && ServerName.IsNotNullOrEmpty() && ((UserName.IsNotNullOrEmpty() && Password.IsNotNullOrEmpty()) || IntegratedSecurity);
        }
        #endregion

        #region [Execute_Reconnect]
        public void Execute_Reconnect(object obj = null)
        {
            try
            {
                if (ServerName.IsNullOrEmpty())
                    ServerName = "localhost";
                if (UserName.IsNotNullOrEmpty())
                    __WriteRegistryValue(nameof(UserName), UserName);
                if (Password.IsNotNullOrEmpty())
                    __WriteRegistryValue(nameof(Password), Password);
                m_db = null;
                skipReload = false;
                Execute_Reload();
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_Reconnect)} failed!", ex.ToString());
                __ResetInvalidConnection(this, new EventArgs());
            }
        }
        #endregion

        #region [Execute_StartMonitoring]
        public void Execute_StartMonitoring(object DatabaseEntry)
        {
            __ThrowMessage($"{nameof(Execute_StartMonitoring)} failed.", "The method is not yet implemented.");

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
                __ThrowMessage($"{nameof(Execute_SelectAll)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_CutLogFile)} failed!", ex.ToString());
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
                __ThrowMessage($"{nameof(Execute_RenameDatabase)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [Execute_ToggleSettings]
        public void Execute_ToggleSettings(object obj = null)
        {
            SettingsOpen = !SettingsOpen;
        }
        #endregion

        #region [Execute_ChangeBaseTheme]
        public void Execute_ChangeBaseTheme(object obj = null)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.GetTheme($"{m_BaseTheme}.{SelectedTheme}")));
            m_BaseTheme = m_BaseTheme.Equals(ThemeManager.BaseColorLight) ? ThemeManager.BaseColorDark : ThemeManager.BaseColorLight;
            __WriteRegistryValue(nameof(m_BaseTheme), m_BaseTheme);
            OnPropertyChanged(nameof(BaseThemeToggled));
        }
        #endregion

        #region [Execute_ChangeTheme]
        public void Execute_ChangeTheme(object obj = null)
        {
            if (SelectedTheme.IsNullOrEmpty())
                return;
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, SelectedTheme);
            __WriteRegistryValue(nameof(SelectedTheme), SelectedTheme);
            OnPropertyChanged(nameof(SelectedTheme));
        }
        #endregion

        #region [Execute_RestartService]
        public void Execute_RestartService(object obj = null)
        {
            ServiceController service = new ServiceController(m_InstanceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(2000);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                DialogManager.ShowModalMessageExternal(MetroWnd, "Successful Restarted", $"SQL Server Instance '{m_InstanceName}' was restarted successful");
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(Execute_RestartService)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [Execute_RestartAsAdmin]
        public void Execute_RestartAsAdmin(object obj = null)
        {
            try
            {
                var Location = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.FullName;
                var Exe = Directory.GetFiles($"{Location}", $"{nameof(DatabaseBuddy)}.exe", SearchOption.AllDirectories);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Exe.First(),
                    Arguments = string.Empty,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                //Do Not Throw here
            }
        }
        #endregion

        #region [Execute_IncreaseScaling]
        public void Execute_IncreaseScaling(object obj = null) => ScalingValue += 0.1;
        #endregion

        #region [Execute_DecreaseScaling]
        public void Execute_DecreaseScaling(object obj = null) => ScalingValue -= 0.1;
        #endregion

        #region [Execute_ResetScaling]
        public void Execute_ResetScaling(object obj = null) => ScalingValue = 1;
        #endregion

        #region - public methods -
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
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion
        #endregion

        #endregion

        #region - private methods -

        #region [__FireChangedEvents]
        private void __FireChangedEvents()
        {
            OnPropertyChanged(nameof(AllDBSize));
            OnPropertyChanged(nameof(AllBackupSize));
            OnPropertyChanged(nameof(AllSize));
        }
        #endregion

        #region [__ResetInvalidConnection]
        private void __ResetInvalidConnection(object sender, EventArgs e)
        {
            if (sender is SqlException SqlEx && SqlEx.Number == 5011)
                return;
            __ThrowMessage("Connection Failed", sender.ToString());
            skipReload = true;
            ServerName = GetRegistryValue(nameof(ServerName));
            Password = string.Empty;
            m_db = null;
            Execute_Reload();
        }
        #endregion

        #region [__GetRegistryValues]
        private void __GetRegistryValues()
        {
            ServerName = GetRegistryValue(nameof(ServerName));
            UserName = GetRegistryValue(nameof(UserName));
            Password = GetRegistryValue(nameof(Password));
            m_ShowSystemDatabases = GetRegistryValue(nameof(ShowSystemDatabases)).ToBooleanValue();
            FileTrackingEnabled = GetRegistryValue("EnableFileSizeMonitoring").ToBooleanValue();
            m_ScheduleActivated = GetRegistryValue("EnabledSchedule").ToBooleanValue();
            m_MSSQLStudioPath = GetRegistryValue(nameof(m_MSSQLStudioPath));
            IntegratedSecurity = GetRegistryValue(nameof(IntegratedSecurity)).ToBooleanValue();
            m_MaxLogSize = GetRegistryValue(nameof(MaxLogSize)).ToLongValue();
            ScalingValue = GetRegistryValue(nameof(ScalingValue)).ToDoubleValue();
            MaxBackupCount = GetRegistryValue(nameof(MaxBackupCount)).ToInt32Value();
            AutoCleanBackups = GetRegistryValue(nameof(AutoCleanBackups)).ToBooleanValue();
        }
        #endregion

        #region [__ThrowMessage]
        private void __ThrowMessage(string Title, string Message)
        {
            (MetroWnd).ShowMessageAsync(Title, Message);
            skipReload = false;
        }
        #endregion

        #region [__HandleTheming]
        private void __HandleTheming()
        {
            m_BaseTheme = GetRegistryValue(nameof(m_BaseTheme));
            SelectedTheme = GetRegistryValue(nameof(SelectedTheme));
            if (m_BaseTheme.IsNullOrEmpty() || SelectedTheme.IsNullOrEmpty())
            {
                __WriteRegistryValue(nameof(m_BaseTheme), ThemeManager.BaseColorLight);
                __WriteRegistryValue(nameof(SelectedTheme), "Blue");
            }
            else
            {
                ThemeManager.Current.ChangeTheme(Application.Current, m_BaseTheme, SelectedTheme);
                OnPropertyChanged(nameof(BaseThemeToggled));
                OnPropertyChanged(nameof(SelectedTheme));
            }
        }
        #endregion

        #region [__GetODBCEntries]
        private static string[] __GetODBCEntries() => Registry.LocalMachine.OpenSubKey(ODBC32BitRegPath).GetSubKeyNames();
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
                SubKey.SetValue("LastUser", m_UserName);
                SubKey.SetValue("Server", ServerName);
            }
            Execute_Reload();
        }
        #endregion

        #region [__MaxFileSizeBox_OkRequested]
        private void __MaxFileSizeBox_OkRequested(object sender, EventArgs e)
        {
            __ThrowMessage($"{nameof(__MaxFileSizeBox_OkRequested)} failed.", "The method is not yet implemented.");
            //m_TrackingMaxFileSizeInput = sender.ToString();
            //var FileSizeDimensionBox = new InputBox("File Size Dimension", "Please choose one of the following Dimensions: B, KB, MB, GB, TB, PB");
            //FileSizeDimensionBox.OkRequested += __FileSizeDimensionBox_OkRequested;
            //FileSizeDimensionBox.ShowDialog();
        }
        #endregion

        #region [__FileSizeDimensionBox_OkRequested]
        private void __FileSizeDimensionBox_OkRequested(object sender, EventArgs e)
        {
            __ThrowMessage($"{nameof(__FileSizeDimensionBox_OkRequested)} failed.", "The method is not yet implemented.");
            //TODO: USE NEW File Checking
            //eDataDimension CurrentDimension;
            //switch (sender.ToStringValue())
            //{
            //    default:
            //    case "B":
            //        CurrentDimension = eDataDimension.Byte;
            //        break;
            //    case "KB":
            //        CurrentDimension = eDataDimension.Kilobyte;
            //        break;
            //    case "MB":
            //        CurrentDimension = eDataDimension.Megabyte;
            //        break;
            //    case "GB":
            //        CurrentDimension = eDataDimension.Gigabyte;
            //        break;
            //    case "TB":
            //        CurrentDimension = eDataDimension.Terabyte;
            //        break;
            //    case "PB":
            //        CurrentDimension = eDataDimension.Petabyte;
            //        break;
            //}
            //m_TrackedFiles.Add(new Entry
            //{
            //    FilePath = m_TrackingDBStateEntry.LDFLocation,
            //    MaxFileSize = m_TrackingMaxFileSizeInput.ToLongValue(),
            //    DataDimension = CurrentDimension
            //});
            //IO.WriteFileList(m_TrackedFiles);
            //Execute_Reload();
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
                  $" -S {ServerName} " +
                  $" -d {DataBaseName}" +
                  (!IntegratedSecurity && m_UserName.IsNotNullOrEmpty() && m_Password.IsNotNullOrEmpty() ?
                  $" -U {m_UserName} " +
                  $"-P {Password}" : " -E") +
                  " -nosplash";
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
                var Variables = Environment.GetEnvironmentVariable("path").Split(';').ToList();
                Variables.Add(@"C:\Program Files (x86)");
                foreach (string folder in Variables)
                {
                    try
                    {
                        var FilesInDirectory = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);
                        var LocatedExe = FilesInDirectory.FirstOrDefault(x => x.EndsWith(ExeName, StringComparison.InvariantCultureIgnoreCase));
                        if (LocatedExe.IsNotNullOrEmpty())
                        {
                            m_MSSQLStudioPath = LocatedExe;
                            __WriteRegistryValue(nameof(m_MSSQLStudioPath), m_MSSQLStudioPath);
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(__GetMSSQLStudioPath)} failed!", ex.ToString());
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
                    Cmd += " AND is_read_only != 1;";
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
                return new List<DBStateEntry>();
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(__LoadDataBases)} failed!", ex.ToString());
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
                        }
                        else
                        {
                            DBEntry.MDFLocation = PathReader["physical_name"].ToStringValue();
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
                    catch (Exception ex)
                    {
                        __ThrowMessage($"{nameof(__KillConnections)} failed!", ex.ToString());
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
                    if (DataBaseName.IsSystemDatabase)
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
            DataBaseEntries.Where(x => !x.IsSystemDatabase).ToList().ForEach(x => Messagetext += $"-{x.DBName}\n");
            if (!silentDelete)
            {
                var DeleteResult = DialogManager.ShowModalMessageExternal(MetroWnd, "Confirm delete", Messagetext, MessageDialogStyle.AffirmativeAndNegative/*, MessageSettings*/);
                if (DeleteResult == MessageDialogResult.Canceled || DeleteResult == MessageDialogResult.Negative)
                    return;
            }
            __KillConnections(DataBaseEntries);

            if (Db != null)
            {
                foreach (var DataBaseName in DataBaseEntries.Where(x => !x.IsSystemDatabase))
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

        #region [__GetLogicalFileNames]
        private Dictionary<string, string> __GetLogicalFileNames(string BakPath)
        {
            string DataFile = string.Empty;
            string LogFile = string.Empty;
            var Cmd = $"RESTORE FILELISTONLY FROM DISK = '{BakPath}'";
            using (var Reader = Db.GetDataReader(Cmd))
            {
                if (Reader.HasRows)
                {
                    while (Reader.Read())
                    {
                        var Type = Reader["Type"].ToStringValue();
                        if (Type.Equals("D"))
                            DataFile = Reader["LogicalName"].ToStringValue();
                        else if (Type.Equals("L"))
                            LogFile = Reader["LogicalName"].ToStringValue();
                    }
                    Reader.Close();
                    Db.CloseDataReader();
                }
            }
            var BackupFileList = new Dictionary<string, string>();
            BackupFileList.Add(nameof(DataFile), DataFile);
            BackupFileList.Add(nameof(LogFile), LogFile);
            return BackupFileList;
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
                DataBaseEntries.Where(x => !x.IsSystemDatabase).ToList().ForEach(x => Messagetext += $"-{x.DBName} Last Backup {x.LastBackupTime}\n");
                if (!SilentRestore)
                {
                    var RestoreBackupResult = DialogManager.ShowModalMessageExternal(MetroWnd, "Confirm restore", Messagetext, MessageDialogStyle.AffirmativeAndNegative/*, MessageSettings*/);
                    if (RestoreBackupResult == MessageDialogResult.Canceled || RestoreBackupResult == MessageDialogResult.Negative)
                        return;
                }
                __KillConnections(DataBaseEntries);

                if (Db != null)
                {
                    foreach (var DataBase in DataBaseEntries)
                    {
                        var BackupPrevData = __GetLogicalFileNames(DataBase.LastBackupPath);
                        if (DataBase.LastBackupPath.Length == 0)
                            continue;
                        var builder = new System.Text.StringBuilder();
                        var Cmd = "USE[master] \n";
                        _ = builder.Append(Cmd);

                        builder.Append($@"RESTORE DATABASE [{DataBase.DBName}]
FROM DISK = N'{DataBase.LastBackupPath}'
WITH
    MOVE '{BackupPrevData["DataFile"]}' TO '{m_DefaultDataPath}{DataBase.DBName}.mdf',
    MOVE '{BackupPrevData["LogFile"]}' TO '{m_DefaultDataPath}{DataBase.DBName}.ldf'");
                        Cmd = builder.ToString();
                        _ = Db.ExecuteNonQuery(Cmd); ;
                    }
                }
                Execute_Reload();
                DialogManager.ShowModalMessageExternal(MetroWnd, $"Successful restored", "Restoring was successful");
            }
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(__RestoreBackup)} failed!", ex.ToString());
                __ActivateConnections(DataBaseEntries);
            }
        }
        #endregion

        #region [__BackupDatabases]
        private void __BackupDatabases(List<DBStateEntry> Entries)
        {
            __ActivateConnections(Entries);
            var BackupTime = DateTime.Now.ToString("ddMMyyyy HHmmss");
            var CmdBackup = "USE [master];\n";
            var builder = new System.Text.StringBuilder();
            _ = builder.Append(CmdBackup);
            if (Db != null)
            {
                foreach (var item in Entries)
                {
                    if (item.DBName.Equals("master", StringComparison.InvariantCultureIgnoreCase)
                        || item.DBName.Equals("tempdb", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    __HandleBackupAutoClean(item);
                    var BackupPath = Path.GetDirectoryName(item.MDFLocation) + $@"\backup\{BackupTime}";
                    _ = Directory.CreateDirectory(BackupPath);
                    _ = builder.Append($@"BACKUP DATABASE [{item.DBName}]
TO DISK = '{BackupPath}\{item.DBName}.bak';");
                }
                CmdBackup = builder.ToString();
                _ = Db.ExecuteNonQuery(CmdBackup);
            }
            DialogManager.ShowModalMessageExternal(MetroWnd, "Backups done", "Backups successful created");
        }

        private void __HandleBackupAutoClean(DBStateEntry item)
        {
            if (!AutoCleanBackups || MaxBackupCount > item.AllBackups.Length)
                return;
            var FileInfos = new List<FileInfo>();
            item.AllBackups.ToList().ForEach(x => FileInfos.Add(new FileInfo(x)));
            FileInfos = FileInfos.OrderBy(x => x.CreationTime.Date).ThenBy(y => y.CreationTime.TimeOfDay).ToList();
            FileInfos.RemoveFrom(FileInfos.Count + 1 - MaxBackupCount);
            FileInfos.ForEach(x => File.Delete(x.FullName));
            ObjectObservation.CleanDirectories($"{Path.GetDirectoryName(item.MDFLocation)}\\backup");
        }
        #endregion

        #region [__GetUnusedDataBaseFiles]
        private void __GetUnusedDataBaseFiles()
        {
            try
            {
                if (!__IsLocal())
                    return;
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
            catch (Exception ex)
            {
                __ThrowMessage($"{nameof(__GetUnusedDataBaseFiles)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [__RunOpenFolder]
        private static void __RunOpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                _ = Process.Start(startInfo);
            }
            else
            {
                _ = DialogManager.ShowModalMessageExternal((MetroWindow)Application.Current.MainWindow, "Directory does not exist", $"{folderPath} Directory does not exist!");
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
        private void __RenameDataBase(List<DBStateEntry> Entries, string Caption = "Choose the new name", string Message = "Please type the new name")
        {
            var Settings = new MetroDialogSettings();
            Settings.DefaultText = Entries.First().DBName;
            var NewName = DialogManager.ShowModalInputExternal(MetroWnd, Caption, Message, Settings)?.Trim();
            if (NewName == null)
                return;
            if (NewName.Equals(""))
                __RenameDataBase(Entries);
            else
            {
                if (SelectedDB is DBStateEntry State && Db != null)
                {
                    if (State.DBName.Equals(NewName, StringComparison.InvariantCultureIgnoreCase) || Db == null)
                        return;
                    if (DBEntries.Any(x => x.DBName.Equals(NewName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        __RenameDataBase(Entries, "Rename failed", $"Database Name {NewName} already exists. Please try again");
                    }
                    else
                    {
                        Db.ExecuteNonQuery($@"USE [MASTER] ALTER DATABASE [{State.DBName}] MODIFY NAME = [{NewName}] ;");
                        Execute_Reload();
                    }
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
                __ThrowMessage($"{nameof(__RunCloneDataBase)} failed!", ex.ToString());
            }
        }
        #endregion

        #region [__RunCreateNewDataBase]
        private void __RunCreateNewDataBase(string Caption = "New database name", string Message = "Please enter your new database name")
        {
            var NewName = DialogManager.ShowModalInputExternal(MetroWnd, Caption, Message)?.Trim();
            if (NewName == null)
                return;
            if (NewName.Equals(""))
                __RunCreateNewDataBase("Retry", "Database Name cannot be empty");
            else
            {
                if (Db == null)
                    return;
                if (DBEntries.Any(x => x.DBName.Equals(NewName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    __RunCreateNewDataBase("Creation failed", $"Database Name {NewName} already exists. Please try again");
                }
                else
                {
                    var Cmd = "USE[master] \n";
                    Cmd += $" CREATE DATABASE [{NewName}];";
                    Db.ExecuteNonQuery(Cmd);
                }
            }
            Execute_Reload();
        }
        #endregion

        #region [__RunCloneDatabaseWithAnyName]
        private void __RunCloneDatabaseWithAnyName(List<DBStateEntry> Entries, string Caption = "Choose Clone name", string Message = "Please type a name for the clone")
        {
            var Settings = new MetroDialogSettings();
            Settings.DefaultText = $"{Entries.First().DBName}_Clone";
            var NewName = DialogManager.ShowModalInputExternal(MetroWnd, Caption, Message, Settings)?.Trim();
            if (NewName == null)
                return;
            if (NewName.Equals(""))
                __RunCloneDatabaseWithAnyName(Entries, "Retry", "Clone Name cannot be empty");
            else
            {
                if (Db == null)
                    return;
                if (DBEntries.Any(x => x.DBName.Equals(NewName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    __RunCloneDatabaseWithAnyName(Entries, "Rename failed", $"Database Name {NewName} already exists. Please try again");
                }
                else
                {
                    SelectedDB.CloneName = NewName;
                    __RunCloneDataBase(new List<DBStateEntry> { SelectedDB });
                    Execute_Reload();
                    DialogManager.ShowModalMessageExternal(MetroWnd, "Successful cloned", $"Successful cloned '{SelectedDB.DBName}' to {SelectedDB.CloneName}");
                }
            }
        }
        #endregion

        #region [__GetVersion]
        private static string __GetVersion()
        {
            return "v " + Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
        #endregion

        #region [__GetDefaultDataPath]
        private void __GetExtendedDBInformations()
        {
            using (var Reader = Db.GetDataReader(@"SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS InstanceDefaultDataPath"))
            {
                if (Reader != null && Reader.HasRows)
                {
                    while (Reader.Read())
                        m_DefaultDataPath = Reader["InstanceDefaultDataPath"].ToStringValue();
                    Reader.Close();
                    Db.CloseDataReader();
                }
            }
            using (var Reader = Db.GetDataReader(@"SELECT @@SERVICENAME AS InstanceName"))
            {
                if (Reader != null && Reader.HasRows)
                {
                    while (Reader.Read())
                        m_InstanceName = Reader["InstanceName"].ToStringValue();
                    Reader.Close();
                    Db.CloseDataReader();
                }
            }
        }
        #endregion

        #region [__GetThemeCboItems]
        private static ObservableCollection<string> __GetThemeCboItems()
        {
            return new ObservableCollection<string>(ThemeManager.Current.ColorSchemes);
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
            ToggleSettings = new DelegateCommand<object>(Execute_ToggleSettings);
            ChangeBaseTheme = new DelegateCommand<object>(Execute_ChangeBaseTheme);
            RestartSQLServerInstance = new DelegateCommand<object>(Execute_RestartService);
            RestartAsAdmin = new DelegateCommand<object>(Execute_RestartAsAdmin);
            IncreaseScaling = new DelegateCommand<object>(Execute_IncreaseScaling);
            DecreaseScaling = new DelegateCommand<object>(Execute_DecreaseScaling);
            ResetScaling = new DelegateCommand<object>(Execute_ResetScaling);
        }
        #endregion

        #endregion
    }
}
