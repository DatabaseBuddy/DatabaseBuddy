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
  public class TestOfNumericExtender
  {
    #region [TestOfToLongValue]
    [TestMethod]
    public void TestOfToLongValue()
    {
      object NullObject = null;
      object LongValueString = "12345";
      object NegativeValueString = "-12345";
      object LongValueMaxString = "9223372036854775807";
      object LongValueMinString = "-9223372036854775808";
      long? NullableLongWithValue = 1234;
      long? NullableLongWithoutValue = null;
      object WhiteSpaceString = "       ";
      object EmptyString = string.Empty;

      Assert.AreEqual(0L, NullObject.ToLongValue(0L));
      Assert.AreEqual(12345L, LongValueString.ToLongValue(0L));
      Assert.AreEqual(-12345L, NegativeValueString.ToLongValue(0L));
      Assert.AreEqual(long.MaxValue, LongValueMaxString.ToLongValue(0L));
      Assert.AreEqual(long.MinValue, LongValueMinString.ToLongValue(0L));
      Assert.AreEqual(1234L, NullableLongWithValue.ToLongValue(0L));
      Assert.AreEqual(0L, NullableLongWithoutValue.ToLongValue(0L));
      Assert.AreEqual(0L, WhiteSpaceString.ToLongValue());
      Assert.AreEqual(0L, EmptyString.ToLongValue());
    }
    #endregion

    #region [TestOfToDoubleValue]
    [TestMethod]
    public void TestOfToDoubleValue()
    {
      object NullObject = null;
      object DoubleValueString = "12345";
      object NegativeValueString = "-12345";
      object DoubleValueMaxString = double.MaxValue.ToString();
      object DoubleValueMinString = double.MinValue.ToString();
      double? NullableDoubleWithValue = 1234;
      double? NullableDoubleWithoutValue = null;
      object WhiteSpaceString = "       ";
      object EmptyString = string.Empty;

      Assert.AreEqual(0D, NullObject.ToDoubleValue(0D));
      Assert.AreEqual(12345D, DoubleValueString.ToDoubleValue(0D));
      Assert.AreEqual(-12345D, NegativeValueString.ToDoubleValue(0D));
      Assert.AreEqual(double.MaxValue, DoubleValueMaxString.ToDoubleValue(0D));
      Assert.AreEqual(double.MinValue, DoubleValueMinString.ToDoubleValue(0D));
      Assert.AreEqual(1234D, NullableDoubleWithValue.ToDoubleValue(0D));
      Assert.AreEqual(0D, NullableDoubleWithoutValue.ToDoubleValue(0D));
      Assert.AreEqual(0D, WhiteSpaceString.ToDoubleValue());
      Assert.AreEqual(0D, EmptyString.ToDoubleValue());
    }
    #endregion

    #region [TestOfToInt32Value]
    [TestMethod]
    public void TestOfToInt32Value()
    {
      object NullObject = null;
      object Int32ValueString = "12345";
      object NegativeValueString = "-12345";
      object Int32ValueMaxString = int.MaxValue.ToString();
      object Int32ValueMinString = int.MinValue.ToString();
      int? NullableIntWithValue = 1234;
      int? NullableIntWithoutValue = null;
      object WhiteSpaceString = "       ";
      object EmptyString = string.Empty;

      Assert.AreEqual(0, NullObject.ToInt32Value(0));
      Assert.AreEqual(12345, Int32ValueString.ToInt32Value(0));
      Assert.AreEqual(-12345, NegativeValueString.ToInt32Value(0));
      Assert.AreEqual(int.MaxValue, Int32ValueMaxString.ToInt32Value(0));
      Assert.AreEqual(int.MinValue, Int32ValueMinString.ToInt32Value(0));
      Assert.AreEqual(1234, NullableIntWithValue.ToInt32Value(0));
      Assert.AreEqual(0, NullableIntWithoutValue.ToInt32Value(0));
      Assert.AreEqual(0, WhiteSpaceString.ToInt32Value());
      Assert.AreEqual(0, EmptyString.ToInt32Value());
    }
    #endregion

    #region [TestOfByteToMegabyte]
    [TestMethod]
    public void TestOfByteToMegabyte()
    {
      long HalfAMegabyte = 500000;
      long Megabyte = 1000000;
      long OneAndAHalfMegabyte = 1500000;
      long TenMegabyte = 10000000;
      long NegativeOne = -1;
      long NegativeOneMegabyte = -1000000;

      Assert.AreEqual(0L, HalfAMegabyte.ByteToMegabyte());
      Assert.AreEqual(1L, Megabyte.ByteToMegabyte());
      Assert.AreEqual(1L, OneAndAHalfMegabyte.ByteToMegabyte());
      Assert.AreEqual(10L, TenMegabyte.ByteToMegabyte());
      Assert.AreEqual(0L, NegativeOne.ByteToMegabyte());
      Assert.AreEqual(0L, NegativeOneMegabyte.ByteToMegabyte());
    }
    #endregion

    #region [TestOfByteToGigabyte]
    [TestMethod]
    public void TestOfByteToGigabyte()
    {
      long HalfAGigabyte = 500000000;
      long Gigabyte = 1000000000;
      long OneAndAHalfGigabyte = 1500000000;
      long TenGigabyte = 10000000000;
      long NegativeOne = -1;
      long NegativeOneGigabyte = -1000000000;

      Assert.AreEqual(0.5D, HalfAGigabyte.ByteToGigabyte());
      Assert.AreEqual(1D, Gigabyte.ByteToGigabyte());
      Assert.AreEqual(1.5D, OneAndAHalfGigabyte.ByteToGigabyte());
      Assert.AreEqual(10D, TenGigabyte.ByteToGigabyte());
      Assert.AreEqual(0D, NegativeOne.ByteToGigabyte());
      Assert.AreEqual(0D, NegativeOneGigabyte.ByteToGigabyte());
    }
    #endregion

    #region [TestOfMegaByteToByte]
    [TestMethod]
    public void TestOfMegaByteToByte()
    {
      long Megabyte = 1;
      long TenMegabyte = 10;
      long NegativeOne = -1;

      Assert.AreEqual(1000000L, Megabyte.MegaByteToByte());
      Assert.AreEqual(10000000L, TenMegabyte.MegaByteToByte());
      Assert.AreEqual(0L, NegativeOne.MegaByteToByte());
    }
    #endregion
  }
}
