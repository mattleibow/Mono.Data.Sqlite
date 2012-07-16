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

using System;
using System.Globalization;
using System.Text;

namespace System.Data.SqlTypes
{
    public sealed class SqlChars : INullable
    {
        #region Fields

        private bool notNull;
        private char[] buffer;
        private StorageState storage = StorageState.UnmanagedBuffer;

        #endregion

        #region Constructors

        public SqlChars()
        {
            notNull = false;
            buffer = null;
        }

        public SqlChars(char[] buffer)
        {
            if (buffer == null)
            {
                notNull = false;
                this.buffer = null;
            }
            else
            {
                notNull = true;
                this.buffer = buffer;
                storage = StorageState.Buffer;
            }
        }

        public SqlChars(SqlString value)
        {
            if (value.IsNull)
            {
                notNull = false;
                buffer = null;
            }
            else
            {
                notNull = true;
                buffer = value.Value.ToCharArray();
                storage = StorageState.Buffer;
            }
        }

        #endregion

        #region Properties

        public char[] Buffer
        {
            get { return buffer; }
        }

        public bool IsNull
        {
            get { return !notNull; }
        }

        public char this[long offset]
        {
            set
            {
                if (notNull && offset >= 0 && offset < buffer.Length)
                    buffer[offset] = value;
            }
            get
            {
                if (buffer == null)
                    throw new SqlNullValueException("Data is Null");
                if (offset < 0 || offset >= buffer.Length)
                    throw new ArgumentOutOfRangeException("Parameter name: offset");
                return buffer[offset];
            }
        }

        public long Length
        {
            get
            {
                if (!notNull || buffer == null)
                    throw new SqlNullValueException("Data is Null");
                if (buffer.Length < 0)
                    return -1;
                return buffer.Length;
            }
        }

        public long MaxLength
        {
            get
            {
                if (!notNull || buffer == null || storage == StorageState.Stream)
                    return -1;
                return buffer.Length;
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
                if (storage == StorageState.UnmanagedBuffer)
                    throw new SqlNullValueException("Data is Null");
                return storage;
            }
        }

        public char[] Value
        {
            get
            {
                if (buffer == null)
                    return buffer;
                return (char[]) buffer.Clone();
            }
        }

        #endregion

        #region Methods

        public void SetLength(long value)
        {
            if (buffer == null)
                throw new SqlTypeException("There is no buffer");
            if (value < 0 || value > buffer.Length)
                throw new ArgumentOutOfRangeException("Specified argument was out of the range of valid values.");
            Array.Resize(ref buffer, (int) value);
        }

        public void SetNull()
        {
            buffer = null;
            notNull = false;
        }

        public static explicit operator SqlString(SqlChars value)
        {
            if (value.IsNull)
                return SqlString.Null;
            else
            {
                return new SqlString(new String(value.Value));
            }
        }

        public static explicit operator SqlChars(SqlString value)
        {
            if (value.IsNull)
                return Null;
            else
                return new SqlChars(value.Value);
        }

        public SqlString ToSqlString()
        {
            if (buffer == null)
            {
                return SqlString.Null;
            }
            else
            {
                return new SqlString(buffer.ToString());
            }
        }

        public long Read(long offset, char[] buffer, int offsetInBuffer, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (IsNull)
                throw new SqlNullValueException("There is no buffer. Read or write operation failed");

            if (count > MaxLength || count > buffer.Length ||
                count < 0 || ((offsetInBuffer + count) > buffer.Length))
                throw new ArgumentOutOfRangeException("count");

            if (offset < 0 || offset > MaxLength)
                throw new ArgumentOutOfRangeException("offset");

            if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length)
                throw new ArgumentOutOfRangeException("offsetInBuffer");

            /*	LAMESPEC: If count specifies more characters than what is available from 
				offset to the Length of the SqlChars instance, only the available 
				characters are copied 
			 */

            /* Final count of what will be copied */
            long actualCount = count;
            if (count + offset > Length)
                actualCount = Length - offset;

            Array.Copy(this.buffer, (int)offset, buffer, offsetInBuffer, (int)actualCount);

            return actualCount;
        }

        public void Write(long offset, char[] buffer, int offsetInBuffer, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (IsNull)
                throw new SqlTypeException("There is no buffer. Read or write operation failed.");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length
                || offsetInBuffer > Length
                || offsetInBuffer + count > Length
                || offsetInBuffer + count > buffer.Length)
                throw new ArgumentOutOfRangeException("offsetInBuffer");

            if (count < 0 || count > MaxLength)
                throw new ArgumentOutOfRangeException("count");

            if (offset > MaxLength || offset + count > MaxLength)
                throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");

            if (count + offset > Length &&
                count + offset <= MaxLength)
                SetLength(count);

            Array.Copy(buffer, offsetInBuffer, this.buffer, (int)offset, count);
        }

        #endregion
    }
}

#endif