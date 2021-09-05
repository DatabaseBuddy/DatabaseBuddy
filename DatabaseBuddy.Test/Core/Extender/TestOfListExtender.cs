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
      List<int> TestList = new List<int>();
      for (int i = 0; i < 100; i++)
        TestList.Add(i);
      TestList.RemoveFrom(0);
      Assert.AreEqual(0, TestList.Count);
    }
    [TestMethod]
    public void TestOfRemoveFrom_10()
    {
      List<int> TestList = new List<int>();
      for (int i = 0; i < 100; i++)
        TestList.Add(i);
      TestList.RemoveFrom(10);
      Assert.AreEqual(10, TestList.Count);
      Assert.IsTrue(TestList.Min() == 0);
      Assert.IsTrue(TestList.Max() == 9);
    }
    [TestMethod]
    public void TestOfRemoveFrom_50()
    {
      List<int> TestList = new List<int>();
      for (int i = 0; i < 100; i++)
        TestList.Add(i);
      TestList.RemoveFrom(50);
      Assert.AreEqual(50, TestList.Count);
      Assert.IsTrue(TestList.Min() == 0);
      Assert.IsTrue(TestList.Max() == 49);
    }
    [TestMethod]
    public void TestOfRemoveFrom_100()
    {
      List<int> TestList = new List<int>();
      for (int i = 0; i < 100; i++)
        TestList.Add(i);
      TestList.RemoveFrom(100);
      Assert.AreEqual(100, TestList.Count);
      Assert.IsTrue(TestList.Min() == 0);
      Assert.IsTrue(TestList.Max() == 99);
    }
    [TestMethod]
    public void TestOfRemoveFrom_NegativeOne()
    {
      List<int> TestList = new List<int>();
      for (int i = 0; i < 100; i++)
        TestList.Add(i);
      TestList.RemoveFrom(-1);
      Assert.AreEqual(100, TestList.Count);
      Assert.IsTrue(TestList.Min() == 0);
      Assert.IsTrue(TestList.Max() == 99);
    }
    #endregion
  }
}
