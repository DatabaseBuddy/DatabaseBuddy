namespace DatabaseBuddy.ViewModel
{
  public static class Statements
  {

    public const string GETLASTBACKUP = "WITH LastBackUp AS\r\n(\r\nSELECT  bs.database_name,\r\n        bs.backup_size,\r\n        bs.backup_start_date,\r\n        bmf.physical_device_name,\r\n        Position = ROW_NUMBER() OVER( PARTITION BY bs.database_name ORDER BY bs.backup_start_date DESC )\r\nFROM  msdb.dbo.backupmediafamily bmf\r\nJOIN msdb.dbo.backupmediaset bms ON bmf.media_set_id = bms.media_set_id\r\nJOIN msdb.dbo.backupset bs ON bms.media_set_id = bs.media_set_id\r\nWHERE   bs.[type] = 'D'\r\nAND bs.is_copy_only = 0\r\n)\r\nSELECT \r\n        sd.name AS [Database],\r\n        CAST(backup_size / 1048576 AS DECIMAL(10, 2) ) AS [BackupSizeMB],\r\n        backup_start_date AS [LastFullDBBackupDate],\r\n        physical_device_name AS [BackupFileLocation]\r\nFROM sys.databases AS sd\r\nLEFT JOIN LastBackUp AS lb\r\n    ON sd.name = lb.database_name\r\n    AND Position = 1\r\n\tWHERE sd.name = @DataBaseName";
    public const string GETACTIVECONNECTIONS = "SELECT CAST(session_id AS VARCHAR(10)) as ID\r\nFROM sys.dm_exec_sessions\r\nWHERE is_user_process = 1 AND program_name <> 'Core .Net SqlClient Data Provider';";
    public const string GETLASTBACKUPDATA = "\r\nUSE [master];\r\nSELECT d.name AS DataBaseName, d.database_id, d.state_desc, f.physical_name, f.type_desc,\r\n(SELECT ROUND(CAST(f.size AS bigint) * 8 / 1024, 0)) as Size\r\nFROM sys.databases d\r\nJOIN sys.master_files f\r\nON d.database_id = f.database_id\r\nWHERE d.name = @DataBaseName;";


  }
}
