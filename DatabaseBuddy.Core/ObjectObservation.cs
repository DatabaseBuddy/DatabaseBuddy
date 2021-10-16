using DatabaseBuddy.Core.Entities;
using DatabaseBuddy.Core.Extender;
using System.IO;
using System.Security.Principal;

namespace DatabaseBuddy.Core
{
  public static class ObjectObservation
  {
    #region - public methods -
    #region [GetBackups]
    public static string[] GetBackups(this DBStateEntryBase Entry, bool IsAdminMode)
    {
      if (!IsAdminMode)
        return new string[0];
      if (File.Exists(Entry.MDFLocation))
      {
        var Backups = Directory.GetFiles($"{Path.GetDirectoryName(Entry.MDFLocation)}", $"*{Entry.DBName}.bak", SearchOption.AllDirectories);
        Entry.AllBackupSize = GetAllBackupFileSize(Backups, IsAdminMode).ByteToMegabyte();
        return Backups;
      }
      return new string[0];
    }
    #endregion

    #region [GetAllBackupFileSize]
    public static long GetAllBackupFileSize(string[] BackupPaths, bool IsAdminMode)
    {
      if (!IsAdminMode)
        return 0;
      long FileSize = 0;
      foreach (var FilePath in BackupPaths)
      {
        if (!File.Exists(FilePath))
          continue;
        else
          FileSize += new FileInfo(FilePath).Length;
      }
      return FileSize;
    }
    #endregion

    #region [CleanDirectories]
    public static void CleanDirectories(string StartLocation, bool IsAdminMode)
    {
      if (!IsAdminMode)
        return;
      if (Directory.Exists(StartLocation))
      {
        foreach (var directory in Directory.GetDirectories(StartLocation))
        {
          CleanDirectories(directory, IsAdminMode);
          if (Directory.GetFiles(directory).Length == 0 &&
              Directory.GetDirectories(directory).Length == 0)
          {
            Directory.Delete(directory, false);
          }
        }
      }
    }
    #endregion
    #endregion
  }
}
