// SqlStringTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlString
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen
// 
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data.SqlTypes;
using System.Globalization;
#if NET_2_0
using System.IO;
#endif
using System.Threading;

#if SILVERLIGHT && !WINDOWS_PHONE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace MonoTests.System.Data.SqlTypes
{
	[TestClass]
	public class SqlStringTest
	{
		private SqlString Test1;
		private SqlString Test2;
		private SqlString Test3;

		[TestInitialize]
		public void GetReady()
		{
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-AU";
			Test1 = new SqlString ("First TestString");
			Test2 = new SqlString ("This is just a test SqlString");
			Test3 = new SqlString ("This is just a test SqlString");
		}

		// Test constructor
		[TestMethod]
		public void Create()
		{
			// SqlString (String)
			SqlString TestString = new SqlString ("Test");
			Assert.AreEqual ("Test", TestString.Value, "#A01");

			// SqlString (String, int)
			TestString = new SqlString ("Test", "en-GB");
			Assert.AreEqual ("en-GB", TestString.Name, "#A02");

			// SqlString (int, SqlCompareOptions, byte[])
			TestString = new SqlString ("en-GB",
				SqlCompareOptions.BinarySort|SqlCompareOptions.IgnoreCase,
				new byte [2] {123, 221});
			Assert.AreEqual ("en-GB", TestString.CompareInfo.Name, "#A03");

			// SqlString(string, int, SqlCompareOptions)
			TestString = new SqlString ("Test", "en-GB", SqlCompareOptions.IgnoreNonSpace);
			Assert.IsTrue (!TestString.IsNull, "#A04");

			// SqlString (int, SqlCompareOptions, byte[], int, int)
			TestString = new SqlString ("en-GB", SqlCompareOptions.BinarySort, new byte [2] {113, 100}, 0, 2);
			Assert.IsTrue (!TestString.IsNull, "#A07");

			// SqlString (int, SqlCompareOptions, byte[], int, int, bool)
			TestString = new SqlString ("en-GB", SqlCompareOptions.IgnoreCase, new byte [3] {123, 111, 222}, 1, 2);
			Assert.IsTrue (!TestString.IsNull, "#A09");
		}

		[TestMethod]
		public void CtorArgumentOutOfRangeException1 ()
		{
		    ExceptionAssert.Throws<ArgumentOutOfRangeException>(
		        delegate
		            {
		                SqlString TestString = new SqlString("en-GB", SqlCompareOptions.BinarySort, new byte[2] {113, 100}, 2, 1);
		            });
		}

		[TestMethod]
		public void CtorArgumentOutOfRangeException2 ()
		{
		    ExceptionAssert.Throws<ArgumentOutOfRangeException>(
		        delegate
		            {
		                SqlString TestString = new SqlString("en-GB", SqlCompareOptions.BinarySort, new byte[2] {113, 100}, 0, 4);
		            });
		}

		// Test public fields
		[TestMethod]
		public void PublicFields()
		{
			// BinarySort
			Assert.AreEqual (32768, SqlString.BinarySort, "#B01");

			// IgnoreCase
			Assert.AreEqual (1, SqlString.IgnoreCase, "#B02");

			// IgnoreKanaType
			Assert.AreEqual (8, SqlString.IgnoreKanaType, "#B03");

			// IgnoreNonSpace
			Assert.AreEqual (2, SqlString.IgnoreNonSpace, "#B04");

			// IgnoreWidth
			Assert.AreEqual (16, SqlString.IgnoreWidth, "#B05");

			// Null
			Assert.IsTrue (SqlString.Null.IsNull, "#B06");
		}

		// Test properties
		[TestMethod]
		public void GlobalizationProperties()
		{
			// CompareInfo
			Assert.AreEqual ("en-AU", Test1.CompareInfo.Name, "#C01");

			// CultureInfo
			Assert.AreEqual ("en-AU", Test1.CultureInfo.Name, "#C02");

			// Name
			Assert.AreEqual ("en-AU", Test1.Name, "#C05");
		}

		// Test properties
		[TestMethod]
		public void Properties()
		{
			// IsNull
			Assert.IsTrue (!Test1.IsNull, "#C03");
			Assert.IsTrue (SqlString.Null.IsNull, "#C04");

			// SqlCompareOptions
			Assert.AreEqual ("IgnoreCase, IgnoreKanaType, IgnoreWidth", 
				Test1.SqlCompareOptions.ToString (), "#C06");

			// Value
			Assert.AreEqual ("First TestString", Test1.Value, "#C07");
		}

		// PUBLIC METHODS

		[TestMethod]
		public void CompareToArgumentException ()
		{
		    ExceptionAssert.Throws<ArgumentException>(
		        delegate
		            {
		                SqlByte Test = new SqlByte(1);
		                Test1.CompareTo(Test);
		            });
		}

		[TestMethod]
		public void CompareToSqlTypeException ()
		{
		    ExceptionAssert.Throws<SqlTypeException>(
		        delegate
		            {
		                SqlString T1 = new SqlString("test", "en-GB", SqlCompareOptions.IgnoreCase);
		                SqlString T2 = new SqlString("TEST", "en-GB", SqlCompareOptions.None);
		                T1.CompareTo(T2);
		            });
		}

		[TestMethod]
#if TARGET_JVM
		[Ignore ("The option CompareOptions.IgnoreWidth is not supported")]
#endif
		public void CompareTo()
		{
			SqlByte Test = new SqlByte (1);

			Assert.IsTrue (Test1.CompareTo (Test3) < 0, "#D01");
			Assert.IsTrue (Test2.CompareTo (Test1) > 0, "#D02");
			Assert.IsTrue (Test2.CompareTo (Test3) == 0, "#D03");
			Assert.IsTrue (Test3.CompareTo (SqlString.Null) > 0, "#D04");

			SqlString T1 = new SqlString ("test", "en-GB", SqlCompareOptions.IgnoreCase);
			SqlString T2 = new SqlString ("TEST", "en-GB", SqlCompareOptions.None);

			// IgnoreCase
			T1 = new SqlString ("test", "en-GB", SqlCompareOptions.IgnoreCase);
			T2 = new SqlString ("TEST", "en-GB", SqlCompareOptions.IgnoreCase);
			Assert.IsTrue (T2.CompareTo (T1) == 0, "#D09");

			T1 = new SqlString ("test", "en-GB");
			T2 = new SqlString ("TEST", "en-GB");
			Assert.IsTrue (T2.CompareTo (T1) == 0, "#D10");

			T1 = new SqlString ("test", "en-GB", SqlCompareOptions.None);
			T2 = new SqlString ("TEST", "en-GB", SqlCompareOptions.None);
			Assert.IsTrue (T2.CompareTo (T1) != 0, "#D11");

			// IgnoreNonSpace
			T1 = new SqlString ("TEST\xF1", "en-GB", SqlCompareOptions.IgnoreNonSpace);
			T2 = new SqlString ("TESTn", "en-GB", SqlCompareOptions.IgnoreNonSpace);
			Assert.IsTrue (T2.CompareTo (T1) == 0, "#D12");

			T1 = new SqlString ("TESTñ", "en-GB", SqlCompareOptions.None);
			T2 = new SqlString ("TESTn", "en-GB", SqlCompareOptions.None);
			Assert.IsTrue (T2.CompareTo (T1) != 0, "#D13");

			// BinarySort
			T1 = new SqlString ("01_", "en-GB", SqlCompareOptions.BinarySort);
			T2 = new SqlString ("_01", "en-GB", SqlCompareOptions.BinarySort);
			Assert.IsTrue (T1.CompareTo (T2) < 0, "#D14");

			T1 = new SqlString ("01_", "en-GB", SqlCompareOptions.None);
			T2 = new SqlString ("_01", "en-GB", SqlCompareOptions.None);
			Assert.IsTrue (T1.CompareTo (T2) > 0, "#D15");
		}

		[TestMethod]
		public void EqualsMethods()
		{
			Assert.IsTrue (!Test1.Equals (Test2), "#E01");
			Assert.IsTrue (!Test3.Equals (Test1), "#E02");
			Assert.IsTrue (!Test2.Equals (new SqlString ("TEST")), "#E03");
			Assert.IsTrue (Test2.Equals (Test3), "#E04");

			// Static Equals()-method
			Assert.IsTrue (SqlString.Equals (Test2, Test3).Value, "#E05");
			Assert.IsTrue (!SqlString.Equals (Test1, Test2).Value, "#E06");
		}

		[TestMethod]
		public void GetHashCodeTest()
		{
			// FIXME: Better way to test HashCode
			Assert.AreEqual (Test1.GetHashCode (), 
				Test1.GetHashCode (), "#F01");
			Assert.IsTrue (Test1.GetHashCode () != Test2.GetHashCode (), "#F02");
			Assert.IsTrue (Test2.GetHashCode () == Test2.GetHashCode (), "#F03");
		}

		[TestMethod]
		public void GetTypeTest()
		{
			Assert.AreEqual ("System.Data.SqlTypes.SqlString", 
				Test1.GetType ().ToString (), "#G01");
			Assert.AreEqual ("System.String", 
				Test1.Value.GetType ().ToString (), "#G02");
		}

		[TestMethod]
#if TARGET_JVM
		[Ignore ("The option CompareOptions.IgnoreWidth is not supported")]
#endif
		public void Greaters()
		{
			// GreateThan ()
			Assert.IsTrue (!SqlString.GreaterThan (Test1, Test2).Value, "#H01");
			Assert.IsTrue (SqlString.GreaterThan (Test2, Test1).Value, "#H02");
			Assert.IsTrue (!SqlString.GreaterThan (Test2, Test3).Value, "#H03");

			// GreaterTharOrEqual ()
			Assert.IsTrue (!SqlString.GreaterThanOrEqual (Test1, Test2).Value, "#H04");
			Assert.IsTrue (SqlString.GreaterThanOrEqual (Test2, Test1).Value, "#H05");
			Assert.IsTrue (SqlString.GreaterThanOrEqual (Test2, Test3).Value, "#H06");
		}

		[TestMethod]
#if TARGET_JVM
		[Ignore ("The option CompareOptions.IgnoreWidth is not supported")]
#endif
		public void Lessers()
		{
			// LessThan()
			Assert.IsTrue (!SqlString.LessThan (Test2, Test3).Value, "#I01");
			Assert.IsTrue (!SqlString.LessThan (Test2, Test1).Value, "#I02");
			Assert.IsTrue (SqlString.LessThan (Test1, Test2).Value, "#I03");

			// LessThanOrEqual ()
			Assert.IsTrue (SqlString.LessThanOrEqual (Test1, Test2).Value, "#I04");
			Assert.IsTrue (!SqlString.LessThanOrEqual (Test2, Test1).Value, "#I05");
			Assert.IsTrue (SqlString.LessThanOrEqual (Test3, Test2).Value, "#I06");
			Assert.IsTrue (SqlString.LessThanOrEqual (Test2, SqlString.Null).IsNull, "#I07");
		}

		[TestMethod]
		public void NotEquals()
		{
			Assert.IsTrue (SqlString.NotEquals (Test1, Test2).Value, "#J01");
			Assert.IsTrue (SqlString.NotEquals (Test2, Test1).Value, "#J02");
			Assert.IsTrue (SqlString.NotEquals (Test3, Test1).Value, "#J03");
			Assert.IsTrue (!SqlString.NotEquals (Test2, Test3).Value, "#J04");
			Assert.IsTrue (SqlString.NotEquals (SqlString.Null, Test3).IsNull, "#J05");
		}

		[TestMethod]
		public void Concat()
		{
			Test1 = new SqlString ("First TestString");
			Test2 = new SqlString ("This is just a test SqlString");
			Test3 = new SqlString ("This is just a test SqlString");

			Assert.AreEqual ((SqlString)"First TestStringThis is just a test SqlString", 
				SqlString.Concat (Test1, Test2), "#K01");

			Assert.AreEqual (SqlString.Null, 
				SqlString.Concat (Test1, SqlString.Null), "#K02");
		}

		[TestMethod]
		public void Clone()
		{
			SqlString TestSqlString = Test1.Clone ();
			Assert.AreEqual (Test1, TestSqlString, "#L01");
		}

		[TestMethod]
		public void CompareOptionsFromSqlCompareOptions()
		{
			Assert.AreEqual (CompareOptions.IgnoreCase,
				SqlString.CompareOptionsFromSqlCompareOptions (
				SqlCompareOptions.IgnoreCase), "#M01");
			Assert.AreEqual (CompareOptions.IgnoreCase,
				SqlString.CompareOptionsFromSqlCompareOptions (
				SqlCompareOptions.IgnoreCase), "#M02");
			try {
				CompareOptions test = SqlString.CompareOptionsFromSqlCompareOptions (
					SqlCompareOptions.BinarySort);
				Assert.Fail ("#M03");
			} catch (ArgumentOutOfRangeException e) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), e.GetType (), "#M04");
			}
		}

		[TestMethod]
		public void UnicodeBytes()
		{
			Assert.AreEqual ((byte)70, Test1.GetUnicodeBytes () [0], "#N03");
			Assert.AreEqual ((byte)0, Test1.GetUnicodeBytes () [1], "#N03c");
			Assert.AreEqual ((byte)105, Test1.GetUnicodeBytes () [2], "#N03e");
			Assert.AreEqual ((byte)0, Test1.GetUnicodeBytes () [3], "#N03g");
			Assert.AreEqual ((byte)114, Test1.GetUnicodeBytes () [4], "#N03i");

			Assert.AreEqual ((byte)105, Test1.GetUnicodeBytes () [2], "#N04");

			try {
				byte test = Test1.GetUnicodeBytes () [105];
				Assert.Fail ("#N05");
			} catch (Exception e) {
#if TARGET_JVM
				Assert.IsTrue (typeof (IndexOutOfRangeException).IsAssignableFrom(e.GetType()), "#N06");
#else
				Assert.AreEqual (typeof (IndexOutOfRangeException), e.GetType(), "#N06");
#endif
			}
		}

		[TestMethod]
		public void ConversionBoolFormatException1 ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                bool test = Test1.ToSqlBoolean().Value;
		            });
		}

		[TestMethod]
		public void ConversionByteFormatException ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                byte test = Test1.ToSqlByte().Value;
		            });
		}

		[TestMethod]
		public void ConversionDecimalFormatException1 ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                Decimal d = Test1.ToSqlDecimal().Value;
		            });
		}

		[TestMethod]
		public void ConversionDecimalFormatException2 ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlDecimal test = String9E300.ToSqlDecimal();
		            });
		}

		[TestMethod]
		public void ConversionGuidFormatException ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlGuid test = String9E300.ToSqlGuid();
		            });
		}

		[TestMethod]
		public void ConversionInt16FormatException ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlInt16 test = String9E300.ToSqlInt16().Value;
		            });
		}

		[TestMethod]
		public void ConversionInt32FormatException1 ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlInt32 test = String9E300.ToSqlInt32().Value;
		            });
		}

		[TestMethod]
		public void ConversionInt32FormatException2 ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlInt32 test = Test1.ToSqlInt32().Value;
		            });
		}

		[TestMethod]
		public void ConversionInt64FormatException ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlInt64 test = String9E300.ToSqlInt64().Value;
		            });
		}

		[TestMethod]
		public void ConversionIntMoneyFormatException2 ()
		{
		    ExceptionAssert.Throws<FormatException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlMoney test = String9E300.ToSqlMoney().Value;
		            });
		}

		[TestMethod]
		public void ConversionByteOverflowException ()
		{
		    ExceptionAssert.Throws<OverflowException>(
		        delegate
		            {
		                SqlByte b = (new SqlString("2500")).ToSqlByte();
		            });
		}

		[TestMethod]
		public void ConversionDoubleOverflowException ()
		{
		    ExceptionAssert.Throws<OverflowException>(
		        delegate
		            {
		                SqlDouble test = (new SqlString("4e400")).ToSqlDouble();
		            });
		}

		[TestMethod]
		public void ConversionSingleOverflowException ()
		{
		    ExceptionAssert.Throws<OverflowException>(
		        delegate
		            {
		                SqlString String9E300 = new SqlString("9E+300");
		                SqlSingle test = String9E300.ToSqlSingle().Value;
		            });
		}

		[TestMethod]
		public void Conversions()
		{
			SqlString String250 = new SqlString ("250");
			SqlString String9E300 = new SqlString ("9E+300");

			// ToSqlBoolean ()
			Assert.IsTrue ((new SqlString("1")).ToSqlBoolean ().Value, "#O02");
			Assert.IsTrue (!(new SqlString("0")).ToSqlBoolean ().Value, "#O03");
			Assert.IsTrue ((new SqlString("True")).ToSqlBoolean ().Value, "#O04");
			Assert.IsTrue (!(new SqlString("FALSE")).ToSqlBoolean ().Value, "#O05");
			Assert.IsTrue (SqlString.Null.ToSqlBoolean ().IsNull, "#O06");

			// ToSqlByte ()
			Assert.AreEqual ((byte)250, String250.ToSqlByte ().Value, "#O08");

			// ToSqlDateTime
			Assert.AreEqual (10, 
				(new SqlString ("2002-10-10")).ToSqlDateTime ().Value.Day, "#O11");

			// ToSqlDecimal ()
			Assert.AreEqual ((decimal)250, String250.ToSqlDecimal ().Value, "#O16");

			// ToSqlDouble
			Assert.AreEqual ((SqlDouble)9E+300, String9E300.ToSqlDouble (), "#O19");

			// ToSqlGuid
			SqlString TestGuid = new SqlString("11111111-1111-1111-1111-111111111111");
			Assert.AreEqual (new SqlGuid("11111111-1111-1111-1111-111111111111"), TestGuid.ToSqlGuid (), "#O22");

			// ToSqlInt16 ()
			Assert.AreEqual ((short)250, String250.ToSqlInt16 ().Value, "#O24");

			// ToSqlInt32 ()
			Assert.AreEqual ((int)250, String250.ToSqlInt32 ().Value, "#O27");

			// ToSqlInt64 ()
			Assert.AreEqual ((long)250, String250.ToSqlInt64 ().Value, "#O32");

			// ToSqlMoney ()
			Assert.AreEqual (250.0000M, String250.ToSqlMoney ().Value, "#O35");

			// ToSqlSingle ()
			Assert.AreEqual ((float)250, String250.ToSqlSingle ().Value, "#O38");

			// ToString ()
			Assert.AreEqual ("First TestString", Test1.ToString (), "#O41");
		}

		// OPERATORS

		[TestMethod]
		public void ArithmeticOperators()
		{
			SqlString TestString = new SqlString ("...Testing...");
			Assert.AreEqual ((SqlString)"First TestString...Testing...",
				Test1 + TestString, "#P01");
			Assert.AreEqual (SqlString.Null,
				Test1 + SqlString.Null, "#P02");
		}

		[TestMethod]
#if TARGET_JVM
		[Ignore ("The option CompareOptions.IgnoreWidth is not supported")]
#endif
		public void ThanOrEqualOperators()
		{
			// == -operator
			Assert.IsTrue ((Test2 == Test3).Value, "#Q01");
			Assert.IsTrue (!(Test1 == Test2).Value, "#Q02");
			Assert.IsTrue ((Test1 == SqlString.Null).IsNull, "#Q03");

			// != -operator
			Assert.IsTrue (!(Test3 != Test2).Value, "#Q04");
			Assert.IsTrue (!(Test2 != Test3).Value, "#Q05");
			Assert.IsTrue ((Test1 != Test3).Value, "#Q06");
			Assert.IsTrue ((Test1 != SqlString.Null).IsNull, "#Q07");

			// > -operator
			Assert.IsTrue ((Test2 > Test1).Value, "#Q08");
			Assert.IsTrue (!(Test1 > Test3).Value, "#Q09");
			Assert.IsTrue (!(Test2 > Test3).Value, "#Q10");
			Assert.IsTrue ((Test1 > SqlString.Null).IsNull, "#Q11");

			// >= -operator
			Assert.IsTrue (!(Test1 >= Test3).Value, "#Q12");
			Assert.IsTrue ((Test3 >= Test1).Value, "#Q13");
			Assert.IsTrue ((Test2 >= Test3).Value, "#Q14");
			Assert.IsTrue ((Test1 >= SqlString.Null).IsNull, "#Q15");

			// < -operator
			Assert.IsTrue ((Test1 < Test2).Value, "#Q16");
			Assert.IsTrue ((Test1 < Test3).Value, "#Q17");
			Assert.IsTrue (!(Test2 < Test3).Value, "#Q18");
			Assert.IsTrue ((Test1 < SqlString.Null).IsNull, "#Q19");

			// <= -operator
			Assert.IsTrue ((Test1 <= Test3).Value, "#Q20");
			Assert.IsTrue (!(Test3 <= Test1).Value, "#Q21");
			Assert.IsTrue ((Test2 <= Test3).Value, "#Q22");
			Assert.IsTrue ((Test1 <= SqlString.Null).IsNull, "#Q23");
		}

		[TestMethod]
		public void SqlBooleanToSqlString()
		{
			SqlBoolean TestBoolean = new SqlBoolean (true);
			SqlBoolean TestBoolean2 = new SqlBoolean (false);
			SqlString Result;

			Result = (SqlString)TestBoolean;
			Assert.AreEqual ("True", Result.Value, "#R01");

			Result = (SqlString)TestBoolean2;
			Assert.AreEqual ("False", Result.Value, "#R02");

			Result = (SqlString)SqlBoolean.Null;
			Assert.IsTrue (Result.IsNull, "#R03");
		}

		[TestMethod]
		public void SqlByteToBoolean()
		{
			SqlByte TestByte = new SqlByte (250);
			Assert.AreEqual ("250", ((SqlString)TestByte).Value, "#S01");
			try {
				SqlString test = ((SqlString)SqlByte.Null).Value;
				Assert.Fail ("#S02");
			} catch (SqlNullValueException e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#S03");
			}
		}

		[TestMethod]
		public void SqlDateTimeToSqlString()
		{
			SqlDateTime TestTime = new SqlDateTime(2002, 10, 22, 9, 52, 30);
			Assert.AreEqual ("22/10/2002 9:52:30 AM", ((SqlString)TestTime).Value, "#T01");
		}

		[TestMethod]
		public void SqlDecimalToSqlString()
		{
			SqlDecimal TestDecimal = new SqlDecimal (1000.2345);
			Assert.AreEqual ("1000.2345000000000", ((SqlString)TestDecimal).Value, "#U01");
		}

		[TestMethod]
		public void SqlDoubleToSqlString()
		{
			SqlDouble TestDouble = new SqlDouble (64E+64);
			Assert.AreEqual ("6.4E+65", ((SqlString)TestDouble).Value, "#V01");
		}

		[TestMethod]
		public void SqlGuidToSqlString()
		{
			byte [] b = new byte [16];
			b [0] = 100;
			b [1] = 64;
			SqlGuid TestGuid = new SqlGuid (b);

			Assert.AreEqual ("00004064-0000-0000-0000-000000000000", 
				((SqlString)TestGuid).Value, "#W01");
			try {
				SqlString test = ((SqlString)SqlGuid.Null).Value;
				Assert.Fail ("#W02");
			} catch (SqlNullValueException e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType(), "#W03");
			}
		}

		[TestMethod]
		public void SqlInt16ToSqlString()
		{
			SqlInt16 TestInt = new SqlInt16(20012);
			Assert.AreEqual ("20012", ((SqlString)TestInt).Value, "#X01");
			try {
				SqlString test = ((SqlString)SqlInt16.Null).Value;
				Assert.Fail ("#X02");
			} catch (SqlNullValueException e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#X03");
			}
		}

		[TestMethod]
		public void SqlInt32ToSqlString()
		{
			SqlInt32 TestInt = new SqlInt32(-12456);
			Assert.AreEqual ("-12456", ((SqlString)TestInt).Value, "#Y01");
			try {
				SqlString test = ((SqlString)SqlInt32.Null).Value;
				Assert.Fail ("#Y02");
			} catch (SqlNullValueException e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#Y03");
			}
		}

		[TestMethod]
		public void SqlInt64ToSqlString()
		{
			SqlInt64 TestInt = new SqlInt64(10101010);
			Assert.AreEqual ("10101010", ((SqlString)TestInt).Value, "#Z01");
		}

		[TestMethod]
		public void SqlMoneyToSqlString()
		{
			SqlMoney TestMoney = new SqlMoney (646464.6464);
			Assert.AreEqual ("646464.6464", ((SqlString)TestMoney).Value, "#AA01");
		}

		[TestMethod]
		public void SqlSingleToSqlString()
		{
			SqlSingle TestSingle = new SqlSingle (3E+20);
			Assert.AreEqual ("3E+20", ((SqlString)TestSingle).Value, "#AB01");
		}

		[TestMethod]
		public void SqlStringToString()
		{
			Assert.AreEqual ("First TestString",(String)Test1, "#AC01");
		}

		[TestMethod]
		public void StringToSqlString()
		{
			String TestString = "Test String";
			Assert.AreEqual ("Test String", ((SqlString)TestString).Value, "#AD01");
		}

#if NET_2_0
		[TestMethod]
		public void AddSqlString()
		{
			Assert.AreEqual ("First TestStringThis is just a test SqlString", (String)(SqlString.Add(Test1, Test2)), "#AE01");
			Assert.AreEqual ("First TestStringPlainString", (String)(SqlString.Add (Test1, "PlainString")), "#AE02");
			Assert.IsTrue (SqlString.Add (Test1, null).IsNull, "#AE03");
		}
#endif
	}
}
