using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBuddy.Core.Entities
{
  public class DBStateEntryBase
  {
    protected List<string> m_SystemDatabases;

    #region [Ctor]
    /// <summary>
    /// Creates a new instance of the DBStateEntryBase.
    /// </summary>
    public DBStateEntryBase()
    {
      m_SystemDatabases = new List<string> { "master", "tempdb", "model", "msdb" };
    }
    #endregion

    #region - properties -

    #region [DBName]
    public string DBName { get; set; }
    #endregion

    #region [CloneName]
    public string CloneName { get; set; }
    #endregion

    #region [IsSelected]
    public bool IsSelected { get; set; }
    #endregion

    #region [ForMultiMode]
    public bool ForMultiMode { get; set; }
    #endregion

    #region [LastBackupPath]
    public string LastBackupPath { get; set; }
    #endregion

    #region [LastBackupTime]
    public string LastBackupTime { get; set; }
    #endregion

    #region [LDFLocation]
    public string LDFLocation { get; set; }
    #endregion

    #region [LDFSize]
    public long LDFSize => this.GetLDFFileSize();
    #endregion

    #region [MDFLocation]
    public string MDFLocation { get; set; }
    #endregion

    #region [MDFSize]
    public long MDFSize => this.GetMDFFileSize();
    #endregion

    #region [HasODBCEntry]
    public bool HasODBCEntry { get; set; }
    #endregion

    #region [IsOnline]
    public bool IsOnline { get; set; }
    #endregion

    #region [CanDBRestore]
    public bool CanDBRestore => __IsBackupValid();
    #endregion

    #region [CutLogfileText]
    public string CutLogfileText => $"Cut Log Size: {LDFSize} MB";
    #endregion

    #region [RestrictedRights]
    public bool RestrictedRights => !IsSystemDatabase;
    #endregion

    #region [HandleStatusText]
    public string HandleStatusText => IsOnline ? "Take offline" : "Take online";
    #endregion

    #region [HandleODBCState]
    public string HandleODBCState => HasODBCEntry ? "Delete ODBC Entry" : "Create ODBC Entry";
    #endregion

    #region [StateTooltip]
    public string StateTooltip => IsOnline ? "Online" : "Offline";
    #endregion

    #region [IsOnlineToggled]
    public bool IsOnlineToggled => IsOnline;
    #endregion

    #region [IsSystemDatabase]
    public bool IsSystemDatabase => m_SystemDatabases.Contains(DBName);
    #endregion

    #region [DataBaseSize]
    public long DataBaseSize => MDFSize + LDFSize;
    #endregion

    #region [InformationString]
    public string InformationString => $"Name: {DBName}\nData File: {MDFSize} MB \nLog Size: {LDFSize} MB \nSum: {DataBaseSize} MB" +
            $"\nData Location: {MDFLocation} \nLog Location: {LDFLocation}";
    #endregion

    #region [RestoreBackupCaption]
    public string RestoreBackupCaption
    {
      get
      {
        if (LastBackupTime.Length > 0)
        {
          if (!__IsBackupValid()) return LastBackupTime;
          return $"Restore last Backup ({LastBackupTime})";
        }
        return "Found no Backup";
      }
    }
    #endregion

    #region [IsTracked]
    public bool IsTracked { get; set; }
    #endregion

    #endregion

    #region - methods -

    #region - private methods -

    #region [__IsBackupValid]
    private bool __IsBackupValid()
      => DateTime.TryParse(LastBackupTime, out DateTime tryout);
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
