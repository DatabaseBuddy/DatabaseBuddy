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
  public class TestOfListExtender
  {
    #region [TestOfRemoveFrom]
    [TestMethod]
    public void TestOfRemoveFrom_0()
    {
      List<object> TestList = new List<object>();
      for (int i = 0; i < 100; i++)
        TestList.Add(new object());
      TestList.RemoveFrom(0);
      Assert.AreEqual(0, TestList.Count);   
    }
    [TestMethod]
    public void TestOfRemoveFrom_10()
    {
      List<object> TestList = new List<object>();
      for (int i = 0; i < 100; i++)
        TestList.Add(new object());
      TestList.RemoveFrom(10);
      Assert.AreEqual(10, TestList.Count);
    }
    [TestMethod]
    public void TestOfRemoveFrom_50()
    {
      List<object> TestList = new List<object>();
      for (int i = 0; i < 100; i++)
        TestList.Add(new object());
      TestList.RemoveFrom(50);
      Assert.AreEqual(50, TestList.Count);
    }
    [TestMethod]
    public void TestOfRemoveFrom_100()
    {
      List<object> TestList = new List<object>();
      for (int i = 0; i < 100; i++)
        TestList.Add(new object());
      TestList.RemoveFrom(100);
      Assert.AreEqual(100, TestList.Count);
    }
    [TestMethod]
    public void TestOfRemoveFrom_NegativeOne()
    {
      List<object> TestList = new List<object>();
      for (int i = 0; i < 100; i++)
        TestList.Add(new object());
      TestList.RemoveFrom(-1);
      Assert.AreEqual(100, TestList.Count);
    }
    #endregion
  }
}
