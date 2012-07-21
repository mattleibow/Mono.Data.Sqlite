//
// System.Data.Common.DbDataReader.cs
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

namespace System.Data.Common
{
    using System.Collections;

    public abstract class DbDataReader : IDataReader, IDataRecord, IDisposable, IEnumerable
    {
        #region Constructors

        #endregion // Constructors

        #region Properties

        public abstract int Depth { get; }
        public abstract int FieldCount { get; }
        public abstract bool HasRows { get; }
        public abstract bool IsClosed { get; }
        public abstract object this[int ordinal] { get; }
        public abstract object this[string name] { get; }
        public abstract int RecordsAffected { get; }

#if NET_2_0
        public virtual int VisibleFieldCount
        {
            get { return this.FieldCount; }
        }
#endif

        #endregion // Properties

        #region Methods

        public abstract void Close();
        public abstract bool GetBoolean(int ordinal);
        public abstract byte GetByte(int ordinal);
        public abstract long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length);
        public abstract char GetChar(int ordinal);
        public abstract long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length);

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        public DbDataReader GetData(int ordinal)
        {
            return ((DbDataReader) this[ordinal]);
        }

        public abstract string GetDataTypeName(int ordinal);
        public abstract DateTime GetDateTime(int ordinal);
        public abstract decimal GetDecimal(int ordinal);
        public abstract double GetDouble(int ordinal);

        public abstract IEnumerator GetEnumerator();

        public abstract Type GetFieldType(int ordinal);
        public abstract float GetFloat(int ordinal);
        public abstract Guid GetGuid(int ordinal);
        public abstract short GetInt16(int ordinal);
        public abstract int GetInt32(int ordinal);
        public abstract long GetInt64(int ordinal);
        public abstract string GetName(int ordinal);
        public abstract int GetOrdinal(string name);

        public virtual Type GetProviderSpecificFieldType(int ordinal)
        {
            return this.GetFieldType(ordinal);
        }

        public virtual object GetProviderSpecificValue(int ordinal)
        {
            return this.GetValue(ordinal);
        }

        public virtual int GetProviderSpecificValues(object[] values)
        {
            return this.GetValues(values);
        }

        protected virtual DbDataReader GetDbDataReader(int ordinal)
        {
            return ((DbDataReader) this[ordinal]);
        }

        public abstract string GetString(int ordinal);
        public abstract object GetValue(int ordinal);
        public abstract int GetValues(object[] values);

        IDataReader IDataRecord.GetData(int ordinal)
        {
            return ((IDataReader) this).GetData(ordinal);
        }

        public abstract bool IsDBNull(int ordinal);
        public abstract bool NextResult();
        public abstract bool Read();

        #endregion // Methods
    }
}

#endif
