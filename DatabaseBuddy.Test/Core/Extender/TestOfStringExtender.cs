using DatabaseBuddy.Core.Extender;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseBuddy.Test.Core.Extender
{
  [TestClass]
  public class TestOfStringExtender
  {
    #region [TestOfStripLeft]
    [TestMethod]
    public void TestOfStripLeft()
    {
      string HelloWorld = "Hello World";
      Assert.IsTrue(" World".Equals(HelloWorld.StripLeft(5)), "First test failed");
      Assert.IsTrue(string.Empty.Equals(HelloWorld.StripLeft(HelloWorld.Length+1)), "Second test failed");
      Assert.IsTrue(string.Empty.Equals(HelloWorld.StripLeft(-1)), "Third test failed");
      Assert.IsTrue(string.Empty.Equals(HelloWorld.StripLeft(HelloWorld.Length)), "Fourth test failed");
    }
    #endregion

    #region [TestOfIsNullOrEmpty]
    [TestMethod]
    public void TestOfIsNullOrEmpty()
    {
      string EmptyString = string.Empty;
      string NullString = null;
      string ContentString = "Hello World";
      string WhiteSpaceString = "    ";

      Assert.IsTrue(EmptyString.IsNullOrEmpty());
      Assert.IsTrue(NullString.IsNullOrEmpty());
      Assert.IsFalse(ContentString.IsNullOrEmpty());
      Assert.IsFalse(WhiteSpaceString.IsNullOrEmpty());
    }
    #endregion

    #region [TestOfIsNotNullOrEmpty]
    [TestMethod]
    public void TestOfIsNotNullOrEmpty()
    {
      string EmptyString = string.Empty;
      string NullString = null;
      string ContentString = "Hello World";
      string WhiteSpaceString = "    ";

      Assert.IsFalse(EmptyString.IsNotNullOrEmpty());
      Assert.IsFalse(NullString.IsNotNullOrEmpty());
      Assert.IsTrue(ContentString.IsNotNullOrEmpty());
      Assert.IsTrue(WhiteSpaceString.IsNotNullOrEmpty());
    }
    #endregion

    #region [TestOfIsNullOrEmptyOrWhiteSpace]
    [TestMethod]
    public void TestOfIsNullOrEmptyOrWhiteSpace()
    {
      string EmptyString = string.Empty;
      string NullString = null;
      string ContentString = "Hello World";
      string WhiteSpaceString = "    ";

      Assert.IsTrue(EmptyString.IsNullOrEmptyOrWhiteSpace());
      Assert.IsTrue(NullString.IsNullOrEmptyOrWhiteSpace());
      Assert.IsFalse(ContentString.IsNullOrEmptyOrWhiteSpace());
      Assert.IsTrue(WhiteSpaceString.IsNullOrEmptyOrWhiteSpace());
    }
    #endregion

    #region [TestOfIsNotNullOrEmptyOrWhiteSpace]
    [TestMethod]
    public void TestOfIsNotNullOrEmptyOrWhiteSpace()
    {
      string EmptyString = string.Empty;
      string NullString = null;
      string ContentString = "Hello World";
      string WhiteSpaceString = "    ";

      Assert.IsFalse(EmptyString.IsNotNullOrEmptyOrWhiteSpace());
      Assert.IsFalse(NullString.IsNotNullOrEmptyOrWhiteSpace());
      Assert.IsTrue(ContentString.IsNotNullOrEmptyOrWhiteSpace());
      Assert.IsFalse(WhiteSpaceString.IsNotNullOrEmptyOrWhiteSpace());
    }
    #endregion

    #region [TestOfContainsOnlyWhiteSpace]
    [TestMethod]
    public void TestOfContainsOnlyWhiteSpace()
    {
      string EmptyString = string.Empty;
      string NullString = null;
      string ContentString = "Hello World";
      string WhiteSpaceString = "    ";

      Assert.IsFalse(EmptyString.ContainsOnlyWhiteSpace());
      Assert.IsFalse(NullString.ContainsOnlyWhiteSpace());
      Assert.IsFalse(ContentString.ContainsOnlyWhiteSpace());
      Assert.IsTrue(WhiteSpaceString.ContainsOnlyWhiteSpace());
    }
    #endregion

    #region [TestOfContainsNotOnlyWhiteSpace]
    [TestMethod]
    public void TestOfContainsNotOnlyWhiteSpace()
    {
      string EmptyString = string.Empty;
      string NullString = null;
      string ContentString = "Hello World";
      string WhiteSpaceString = "    ";

      Assert.IsTrue(EmptyString.ContainsNotOnlyWhiteSpace());
      Assert.IsTrue(NullString.ContainsNotOnlyWhiteSpace());
      Assert.IsTrue(ContentString.ContainsNotOnlyWhiteSpace());
      Assert.IsFalse(WhiteSpaceString.ContainsNotOnlyWhiteSpace());
    }
    #endregion

    #region [TestOfToBooleanValue]
    [TestMethod]
    public void TestOfToBooleanValue()
    {
      string One = "1";
      string Two = "2";
      string Zero = "0";
      string NulLValue = null;
      string EmptyValue = string.Empty;
      string WhiteSpaceValue = "   ";
      string True = "true";
      string False = "false";

      Assert.IsTrue(One.ToBooleanValue());
      Assert.IsFalse(Two.ToBooleanValue());
      Assert.IsFalse(Zero.ToBooleanValue());
      Assert.IsFalse(NulLValue.ToBooleanValue());
      Assert.IsFalse(EmptyValue.ToBooleanValue());
      Assert.IsFalse(WhiteSpaceValue.ToBooleanValue());
      Assert.IsTrue(True.ToBooleanValue());
      Assert.IsFalse(False.ToBooleanValue());
    }
    #endregion

    #region [TestOfIsValidIPAddress]
    [TestMethod]
    public void TestOfIsValidIPAddress()
    {
      string IP_Valid_1 = "0.0.0.0";
      string IP_Valid_2 = "000.000.000.000";
      string IP_Valid_3 = "0.00.00.000";
      string IP_Valid_4 = "1.1.1.1";
      string IP_Valid_5 = "1.11.11.111";
      string IP_Valid_6 = "123.123.123.123";
      string IP_Valid_7 = "255.255.255.255";
      string IP_Invalid_1 = "-1.0.0.0";
      string IP_Invalid_2 = "-1.-1.-1.-1";
      string IP_Invalid_3 = "-255.-255.-255.-255";
      string IP_Invalid_4 = "1.1.1.256";
      string IP_Invalid_5 = "1.1.256.1";
      string IP_Invalid_6 = "1.256.1.1";
      string IP_Invalid_7 = "256.256.246.266";
      string IP_Invalid_8 = "123.123.123";
      string IP_Invalid_9 = "1 2. 13. 54. 15 2";
      string EmptyString = string.Empty;
      string NullString = null;
      string WhiteSpaceString = "    ";

      Assert.IsTrue(IP_Valid_1.IsValidIPAddress(), nameof(IP_Valid_1));
      Assert.IsTrue(IP_Valid_2.IsValidIPAddress(), nameof(IP_Valid_2));
      Assert.IsTrue(IP_Valid_3.IsValidIPAddress(), nameof(IP_Valid_3));
      Assert.IsTrue(IP_Valid_4.IsValidIPAddress(), nameof(IP_Valid_4));
      Assert.IsTrue(IP_Valid_5.IsValidIPAddress(), nameof(IP_Valid_5));
      Assert.IsTrue(IP_Valid_6.IsValidIPAddress(), nameof(IP_Valid_6));
      Assert.IsTrue(IP_Valid_7.IsValidIPAddress(), nameof(IP_Valid_7));

      Assert.IsFalse(IP_Invalid_1.IsValidIPAddress(), nameof(IP_Invalid_1));
      Assert.IsFalse(IP_Invalid_2.IsValidIPAddress(), nameof(IP_Invalid_2));
      Assert.IsFalse(IP_Invalid_3.IsValidIPAddress(), nameof(IP_Invalid_3));
      Assert.IsFalse(IP_Invalid_4.IsValidIPAddress(), nameof(IP_Invalid_4));
      Assert.IsFalse(IP_Invalid_5.IsValidIPAddress(), nameof(IP_Invalid_5));
      Assert.IsFalse(IP_Invalid_6.IsValidIPAddress(), nameof(IP_Invalid_6));
      Assert.IsFalse(IP_Invalid_7.IsValidIPAddress(), nameof(IP_Invalid_7));
      Assert.IsFalse(IP_Invalid_8.IsValidIPAddress(), nameof(IP_Invalid_8));
      Assert.IsFalse(IP_Invalid_9.IsValidIPAddress(), nameof(IP_Invalid_9));

      Assert.IsFalse(EmptyString.IsValidIPAddress(), nameof(EmptyString));
      Assert.IsFalse(NullString.IsValidIPAddress(), nameof(NullString));
      Assert.IsFalse(WhiteSpaceString.IsValidIPAddress(), nameof(WhiteSpaceString));
    }
    #endregion

  }
}
