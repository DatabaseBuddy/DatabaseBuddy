using DatabaseBuddy.Core.Entities;
using DatabaseBuddy.Core.Extender;
using System.IO;

namespace DatabaseBuddy.Core
{
    public static class ObjectObservation
    {
        #region [GetMDFFileSize]
        public static long GetMDFFileSize(this DBStateEntryBase Entry)
        {
            var FilePath = Entry.MDFLocation;
            if (!File.Exists(FilePath))
                return default(long);
            else
                return new FileInfo(FilePath).Length;
        }
        public static long GetLDFFileSize(this DBStateEntryBase Entry)
        {
            var FilePath = Entry.LDFLocation;
            if (!File.Exists(FilePath))
                return default(long);
            else
                return new FileInfo(FilePath).Length;
        }
        #endregion
        public static string[] GetBackups(this DBStateEntryBase Entry)
        {
            var Backups = Directory.GetFiles($"{Path.GetDirectoryName(Entry.MDFLocation)}", $"*{Entry.DBName}.bak", SearchOption.AllDirectories);
            Entry.AllBackupSize = GetAllBackupFileSize(Backups).ToMegabyte();
            return Backups;
        }

        public static long GetAllBackupFileSize(string[] BackupPaths)
        {
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

        public static void CleanDirectories(string StartLocation)
        {
            foreach (var directory in Directory.GetDirectories(StartLocation))
            {
                CleanDirectories(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}
