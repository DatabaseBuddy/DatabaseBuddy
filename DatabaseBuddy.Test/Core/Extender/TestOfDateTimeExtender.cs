using DatabaseBuddy.Core.Extender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBuddy.Test.Core.Extender
{
  [TestClass]
  public class TestOfDateTimeExtender
  {
    #region [TestOfToDateTimeValue]
    [TestMethod]
    public void TestOfToDateTimeValue()
    {
      //Object NullValue = null;
      //Object DBNullValue = DBNull.Value;
      Object DateTimeValue = new DateTime(1998, 01, 01);
      Object NullableDateTimeValueWithValue = new DateTime(1998, 01, 01);
      //Object NullableDateTimeValueWithoutValue = null;
      Object StringValue = "2021/01/01";

      //Assert.AreEqual(DateTime.Now, NullValue); //Without shims not possible to test.
      //Assert.AreEqual(DateTime.Now, DBNullValue); //Without shims not possible to test.
      Assert.AreEqual(new DateTime(1998, 01, 01), DateTimeValue.ToDateTimeValue()); 
      Assert.AreEqual(new DateTime(1998, 01, 01), NullableDateTimeValueWithValue.ToDateTimeValue());
      //Assert.AreEqual(DateTime.Now, NullableDateTimeValueWithoutValue.ToDateTimeValue()); //Without shims not possible to test.
      Assert.AreEqual(new DateTime(2021, 01, 01), StringValue.ToDateTimeValue());
    }
    #endregion
  }
}
