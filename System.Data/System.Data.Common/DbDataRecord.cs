//
// System.Data.Common.DbDataRecord.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002-2003
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

namespace System.Data.Common
{
    public abstract class DbDataRecord : IDataRecord
    {
        public abstract int FieldCount { get; }
        public abstract object this[string name] { get; }
        public abstract object this[int i] { get; }

        public abstract bool GetBoolean(int i);
        public abstract byte GetByte(int i);
        public abstract long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length);
        public abstract char GetChar(int i);
        public abstract long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length);
        public abstract string GetDataTypeName(int i);
#if NET_2_0
        protected abstract DbDataReader GetDbDataReader(int i);
#endif
        public abstract DateTime GetDateTime(int i);
        public abstract decimal GetDecimal(int i);
        public abstract double GetDouble(int i);
        public abstract Type GetFieldType(int i);
        public abstract float GetFloat(int i);
        public abstract Guid GetGuid(int i);
        public abstract short GetInt16(int i);
        public abstract int GetInt32(int i);
        public abstract long GetInt64(int i);
        public abstract string GetName(int i);
        public abstract int GetOrdinal(string name);
        public abstract string GetString(int i);
        public abstract object GetValue(int i);
        public abstract int GetValues(object[] values);
        public abstract bool IsDBNull(int i);

        public IDataReader GetData(int i)
        {
            return (IDataReader) this.GetValue(i);
        }
    }

    internal class DbDataRecordImpl : DbDataRecord
    {
        #region Fields

        private readonly SchemaInfo[] schema;
        private readonly object[] values;
        private readonly int fieldCount;

        #endregion

        #region Constructors

        // FIXME: this class should actually be reimplemented to be one
        // of the derived classes of DbDataRecord, which should become
        // almost abstract.
        internal DbDataRecordImpl(SchemaInfo[] schema, object[] values)
        {
            this.schema = schema;
            this.values = values;
            this.fieldCount = values.Length;
        }

        #endregion

        #region Properties

        public override int FieldCount
        {
            get { return this.fieldCount; }
        }

        public override object this[string name]
        {
            get { return this[this.GetOrdinal(name)]; }
        }

        public override object this[int i]
        {
            get { return this.GetValue(i); }
        }

        #endregion

        #region Methods

        public override bool GetBoolean(int i)
        {
            return (bool) this.GetValue(i);
        }

        public override byte GetByte(int i)
        {
            return (byte) this.GetValue(i);
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            object value = this.GetValue(i);
            if (!(value is byte[]))
            {
                throw new InvalidCastException("Type is " + value.GetType());
            }

            if (buffer == null)
            {
                // Return length of data
                return ((byte[]) value).Length;
            }
            else
            {
                // Copy data into buffer
                Array.Copy((byte[]) value, (int) dataIndex, buffer, bufferIndex, length);
                return ((byte[]) value).Length - dataIndex;
            }
        }

        public override char GetChar(int i)
        {
            return (char) this.GetValue(i);
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            object value = this.GetValue(i);
            char[] valueBuffer;

            if (value is char[])
            {
                valueBuffer = (char[]) value;
            }
            else if (value is string)
            {
                valueBuffer = ((string) value).ToCharArray();
            }
            else
            {
                throw new InvalidCastException("Type is " + value.GetType());
            }

            if (buffer == null)
            {
                // Return length of data
                return valueBuffer.Length;
            }
            else
            {
                // Copy data into buffer
                Array.Copy(valueBuffer, (int) dataIndex, buffer, bufferIndex, length);
                return valueBuffer.Length - dataIndex;
            }
        }

        public override string GetDataTypeName(int i)
        {
            return this.schema[i].DataTypeName;
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime) this.GetValue(i);
        }

#if NET_2_0
        [MonoTODO]
        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            throw new NotImplementedException();
        }
#endif

        public override decimal GetDecimal(int i)
        {
            return (decimal) this.GetValue(i);
        }

        public override double GetDouble(int i)
        {
            return (double) this.GetValue(i);
        }

        public override Type GetFieldType(int i)
        {
            return this.schema[i].FieldType;
        }

        public override float GetFloat(int i)
        {
            return (float) this.GetValue(i);
        }

        public override Guid GetGuid(int i)
        {
            return (Guid) this.GetValue(i);
        }

        public override short GetInt16(int i)
        {
            return (short) this.GetValue(i);
        }

        public override int GetInt32(int i)
        {
            return (int) this.GetValue(i);
        }

        public override long GetInt64(int i)
        {
            return (long) this.GetValue(i);
        }

        public override string GetName(int i)
        {
            return this.schema[i].ColumnName;
        }

        public override int GetOrdinal(string name)
        {
            for (int i = 0; i < this.FieldCount; i++)
            {
                if (this.schema[i].ColumnName == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public override string GetString(int i)
        {
            return (string) this.GetValue(i);
        }

        public override object GetValue(int i)
        {
            if (i < 0 || i > this.fieldCount)
            {
                throw new IndexOutOfRangeException();
            }
            return this.values[i];
        }

        public override int GetValues(object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            int count = values.Length > this.values.Length ? this.values.Length : values.Length;
            for (int i = 0; i < count; i++)
            {
                values[i] = this.values[i];
            }
            return count;
        }

        public override bool IsDBNull(int i)
        {
            return this.GetValue(i) == DBNull.Value;
        }

        #endregion // Methods
    }
}
