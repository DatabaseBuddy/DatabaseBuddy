using DatabaseBuddy.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  }
}
