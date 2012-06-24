//
// System.Data.Common.DbCommandBuilder
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0 || TARGET_JVM

using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;

namespace System.Data.Common {
	public abstract class DbCommandBuilder : Component
	{
		bool _setAllValues;
		bool _disposed;

		private CatalogLocation _catalogLocation = CatalogLocation.Start;
		private ConflictOption _conflictOption = ConflictOption.CompareAllSearchableValues;

		private string _tableName;
		private string _catalogSeparator;
		private string _quotePrefix;
		private string _quoteSuffix;
		private string _schemaSeparator;

		static readonly string SEPARATOR_DEFAULT = ".";
		// Used to construct WHERE clauses
		static readonly string clause1 = "({0} = 1 AND {1} IS NULL)";
		static readonly string clause2 = "({0} = {1})";

		private string QuotedTableName {
			get { return GetQuotedString (_tableName); }
		}

		private string GetQuotedString (string value)
		{
			if (value == String.Empty || value == null)
				return value;

			string prefix = QuotePrefix;
			string suffix = QuoteSuffix;

			if (prefix.Length == 0 && suffix.Length == 0)
				return value;
			return String.Format ("{0}{1}{2}", prefix, value, suffix);
		}

		[DefaultValue (CatalogLocation.Start)]
		public virtual CatalogLocation CatalogLocation {
			get { return _catalogLocation; }
			set {
				CheckEnumValue (typeof (CatalogLocation),
					(int) value);
				_catalogLocation = value;
			}
		}

		[DefaultValue (".")]
		public virtual string CatalogSeparator {
			get {
				if (_catalogSeparator == null || _catalogSeparator.Length == 0)
					return SEPARATOR_DEFAULT;
				return _catalogSeparator;
			}
			set { _catalogSeparator = value; }
		}

		[DefaultValue (ConflictOption.CompareAllSearchableValues)]
		public virtual ConflictOption ConflictOption {
			get { return _conflictOption; }
			set {
				CheckEnumValue (typeof (ConflictOption),
					(int) value);
				_conflictOption = value;
			}
		}

		[DefaultValue ("")]
		public virtual string QuotePrefix {
			get {
				if (_quotePrefix == null)
					return string.Empty;
				return _quotePrefix;
			}
			set {
				_quotePrefix = value;
			}
		}

		[DefaultValue ("")]
		public virtual string QuoteSuffix {
			get {
				if (_quoteSuffix == null)
					return string.Empty;
				return _quoteSuffix;
			}
			set {
				_quoteSuffix = value;
			}
		}

		[DefaultValue (".")]
		public virtual string SchemaSeparator {
			get {
				if (_schemaSeparator == null || _schemaSeparator.Length == 0)
					return SEPARATOR_DEFAULT;
				return _schemaSeparator;
			}
			set { _schemaSeparator = value; }
		}

		[DefaultValue (false)]
		public bool SetAllValues {
			get { return _setAllValues; }
			set { _setAllValues = value; }
		}

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
				}
				_disposed = true;
			}
		}

		public virtual string QuoteIdentifier (string unquotedIdentifier)
		{
			throw new NotSupportedException ();
		}

		public virtual string UnquoteIdentifier (string quotedIdentifier)
		{
			if (quotedIdentifier == null) {
				throw new ArgumentNullException ("Quoted identifier parameter cannot be null");
			}
			string unquotedIdentifier = quotedIdentifier.Trim ();
			if (unquotedIdentifier.StartsWith (this.QuotePrefix)) {
				unquotedIdentifier = unquotedIdentifier.Remove (0, 1);
			}
			if (unquotedIdentifier.EndsWith (this.QuoteSuffix)) {
				unquotedIdentifier = unquotedIdentifier.Remove (unquotedIdentifier.Length - 1, 1);
			}
			return unquotedIdentifier;
		}

		public virtual void RefreshSchema ()
		{
			_tableName = String.Empty;
		}

		protected abstract string GetParameterName (int parameterOrdinal);
		protected abstract string GetParameterName (String parameterName);
		protected abstract string GetParameterPlaceholder (int parameterOrdinal);
        
		static void CheckEnumValue (Type type, int value)
		{
			if (Enum.IsDefined (type, value))
				return;

			string typename = type.Name;
			string msg = string.Format (CultureInfo.CurrentCulture,
				"Value {0} is not valid for {1}.", value,
				typename);
			throw new ArgumentOutOfRangeException (typename, msg);
		}
	}
}

#endif
