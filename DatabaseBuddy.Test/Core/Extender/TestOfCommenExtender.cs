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
  public class TestOfCommenExtender
  {
    #region [TestOfIsNull]
    [TestMethod]
    public void TestOfIsNull()
    {
      Object NullValue = null;
      Object DBNullValue = DBNull.Value;
      Object Value = new Object();

      Assert.IsTrue(NullValue.IsNull());
      Assert.IsTrue(DBNullValue.IsNull());
      Assert.IsFalse(Value.IsNull());
    }
    #endregion

    #region [TestOfIsNotNull]
    [TestMethod]
    public void TestOfIsNotNull()
    {
      Object NullValue = null;
      Object DBNullValue = DBNull.Value;
      Object Value = new Object();

      Assert.IsFalse(NullValue.IsNotNull());
      Assert.IsFalse(DBNullValue.IsNotNull());
      Assert.IsTrue(Value.IsNotNull());
    }
    #endregion
  }
}
