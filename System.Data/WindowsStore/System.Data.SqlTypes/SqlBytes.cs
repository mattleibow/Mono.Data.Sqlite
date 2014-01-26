//
// System.Data.SqlTypes.SqlBytes
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
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


namespace System.Data.SqlTypes
{
    using System.IO;

    public sealed class SqlBytes : INullable
    {
        #region Fields

        private readonly StorageState storage = StorageState.UnmanagedBuffer;
        private byte[] buffer;
        private bool notNull;

        #endregion

        #region Constructors

        public SqlBytes()
        {
            this.buffer = null;
            this.notNull = false;
        }

        public SqlBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                this.notNull = false;
                buffer = null;
            }
            else
            {
                this.notNull = true;
                this.buffer = buffer;
                this.storage = StorageState.Buffer;
            }
        }

        public SqlBytes(SqlBinary value)
        {
            if (value.IsNull)
            {
                this.notNull = false;
                this.buffer = null;
            }
            else
            {
                this.notNull = true;
                this.buffer = value.Value;
                this.storage = StorageState.Buffer;
            }
        }

        public SqlBytes(Stream s)
        {
            if (s == null)
            {
                this.notNull = false;
                this.buffer = null;
            }
            else
            {
                this.notNull = true;
                var len = (int) s.Length;
                this.buffer = new byte[len];
                s.Read(this.buffer, 0, len);
                this.storage = StorageState.Stream;
                this.Stream = s;
            }
        }

        #endregion

        #region Properties

        public byte[] Buffer
        {
            get { return this.buffer; }
        }

        public byte this[long offset]
        {
            set
            {
                if (this.notNull && offset >= 0 && offset < this.buffer.Length)
                {
                    this.buffer[offset] = value;
                }
            }
            get
            {
                if (this.buffer == null)
                {
                    throw new SqlNullValueException("Data is Null");
                }
                if (offset < 0 || offset >= this.buffer.Length)
                {
                    throw new ArgumentOutOfRangeException("Parameter name: offset");
                }
                return this.buffer[offset];
            }
        }

        public long Length
        {
            get
            {
                if (!this.notNull || this.buffer == null)
                {
                    throw new SqlNullValueException("Data is Null");
                }
                if (this.buffer.Length < 0)
                {
                    return -1;
                }
                return this.buffer.Length;
            }
        }

        public long MaxLength
        {
            get
            {
                if (!this.notNull || this.buffer == null || this.storage == StorageState.Stream)
                {
                    return -1;
                }
                return this.buffer.Length;
            }
        }

        public static SqlBytes Null
        {
            get { return new SqlBytes(); }
        }

        public StorageState Storage
        {
            get
            {
                if (this.storage == StorageState.UnmanagedBuffer)
                {
                    throw new SqlNullValueException("Data is Null");
                }
                return this.storage;
            }
        }

        public Stream Stream { set; get; }

        public byte[] Value
        {
            get
            {
                if (this.buffer == null)
                {
                    return this.buffer;
                }
                return (byte[]) this.buffer.Clone();
            }
        }

        public bool IsNull
        {
            get { return !this.notNull; }
        }

        #endregion

        #region Methods

        public void SetLength(long value)
        {
            if (this.buffer == null)
            {
                throw new SqlTypeException("There is no buffer. Read or write operation failed.");
            }
            if (value < 0 || value > this.buffer.Length)
            {
                throw new ArgumentOutOfRangeException("Specified argument was out of the range of valid values.");
            }
            Array.Resize(ref this.buffer, (int) value);
        }

        public void SetNull()
        {
            this.buffer = null;
            this.notNull = false;
        }

        public static explicit operator SqlBytes(SqlBinary value)
        {
            if (value.IsNull)
            {
                return Null;
            }
            else
            {
                return new SqlBytes(value.Value);
            }
        }

        public static explicit operator SqlBinary(SqlBytes value)
        {
            if (value.IsNull)
            {
                return SqlBinary.Null;
            }
            else
            {
                return new SqlBinary(value.Value);
            }
        }

        public SqlBinary ToSqlBinary()
        {
            return new SqlBinary(this.buffer);
        }

        public long Read(long offset, byte[] buffer, int offsetInBuffer, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (this.IsNull)
            {
                throw new SqlNullValueException("There is no buffer. Read or write failed");
            }

            if (count > this.MaxLength || count > buffer.Length ||
                count < 0 || ((offsetInBuffer + count) > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (offset < 0 || offset > this.MaxLength)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offsetInBuffer");
            }

            /* Final count of what will be copied */
            long actualCount = count;
            if (count + offset > this.Length)
            {
                actualCount = this.Length - offset;
            }

            Array.Copy(this.buffer, (int) offset, buffer, offsetInBuffer, (int) actualCount);

            return actualCount;
        }

        public void Write(long offset, byte[] buffer, int offsetInBuffer, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (this.IsNull)
            {
                throw new SqlTypeException("There is no buffer. Read or write operation failed.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length
                || offsetInBuffer > this.Length
                || offsetInBuffer + count > this.Length
                || offsetInBuffer + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offsetInBuffer");
            }

            if (count < 0 || count > this.MaxLength)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (offset > this.MaxLength || offset + count > this.MaxLength)
            {
                throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");
            }

            if (count + offset > this.Length &&
                count + offset <= this.MaxLength)
            {
                this.SetLength(count);
            }

            Array.Copy(buffer, offsetInBuffer, this.buffer, (int) offset, count);
        }

        #endregion
    }
}
