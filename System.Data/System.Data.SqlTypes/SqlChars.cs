//
// System.Data.SqlTypes.SqlChars
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


#if NET_2_0

namespace System.Data.SqlTypes
{
    public sealed class SqlChars : INullable
    {
        #region Fields

        private readonly StorageState storage = StorageState.UnmanagedBuffer;
        private char[] buffer;
        private bool notNull;

        #endregion

        #region Constructors

        public SqlChars()
        {
            this.notNull = false;
            this.buffer = null;
        }

        public SqlChars(char[] buffer)
        {
            if (buffer == null)
            {
                this.notNull = false;
                this.buffer = null;
            }
            else
            {
                this.notNull = true;
                this.buffer = buffer;
                this.storage = StorageState.Buffer;
            }
        }

        public SqlChars(SqlString value)
        {
            if (value.IsNull)
            {
                this.notNull = false;
                this.buffer = null;
            }
            else
            {
                this.notNull = true;
                this.buffer = value.Value.ToCharArray();
                this.storage = StorageState.Buffer;
            }
        }

        #endregion

        #region Properties

        public char[] Buffer
        {
            get { return this.buffer; }
        }

        public char this[long offset]
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

        public static SqlChars Null
        {
            get { return new SqlChars(); }
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

        public char[] Value
        {
            get
            {
                if (this.buffer == null)
                {
                    return this.buffer;
                }
                return (char[]) this.buffer.Clone();
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
                throw new SqlTypeException("There is no buffer");
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

        public static explicit operator SqlString(SqlChars value)
        {
            if (value.IsNull)
            {
                return SqlString.Null;
            }
            else
            {
                return new SqlString(new String(value.Value));
            }
        }

        public static explicit operator SqlChars(SqlString value)
        {
            if (value.IsNull)
            {
                return Null;
            }
            else
            {
                return new SqlChars(value.Value);
            }
        }

        public SqlString ToSqlString()
        {
            if (this.buffer == null)
            {
                return SqlString.Null;
            }
            else
            {
                return new SqlString(this.buffer.ToString());
            }
        }

        public long Read(long offset, char[] buffer, int offsetInBuffer, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (this.IsNull)
            {
                throw new SqlNullValueException("There is no buffer. Read or write operation failed");
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

            /*	LAMESPEC: If count specifies more characters than what is available from 
				offset to the Length of the SqlChars instance, only the available 
				characters are copied 
			 */

            /* Final count of what will be copied */
            long actualCount = count;
            if (count + offset > this.Length)
            {
                actualCount = this.Length - offset;
            }

            Array.Copy(this.buffer, (int) offset, buffer, offsetInBuffer, (int) actualCount);

            return actualCount;
        }

        public void Write(long offset, char[] buffer, int offsetInBuffer, int count)
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

#endif
