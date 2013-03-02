/*
Copyright (C) 2013 Peter Huene

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#include "pch.h"
#include "UnsafeNativeMethods.h"

using namespace MonoDataSqliteWrapper;
using namespace Platform;
using namespace std;

vector<char> convert_to_utf8_buffer(String^ str)
{
	// A null value cannot be marshalled for Platform::String^, so they should never be null
	if (str->IsEmpty())
	{
		// Return an "empty" string
		return vector<char>(1);
	}

	// Get the size of the utf-8 string
	int size = WideCharToMultiByte(CP_UTF8, WC_ERR_INVALID_CHARS, str->Data(), str->Length(), nullptr, 0, nullptr, nullptr);
	if (size == 0)
	{
		// Not much we can do here; just return an empty string
		return vector<char>(1);
	}

	// Allocate the buffer and do the conversion
	vector<char> buffer(size + 1 /* null */);
	if (WideCharToMultiByte(CP_UTF8, WC_ERR_INVALID_CHARS, str->Data(), str->Length(), buffer.data(), size, nullptr, nullptr) == 0)
	{
		// Not much we can do here; just return an empty string
		return vector<char>(1);
	}

	return std::move(buffer);
}

String^ convert_to_string(char const* str)
{
	if (!str)
	{
		return ref new String();
	}

	// Get the size of the wide string
	int size = MultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, str, -1, nullptr, 0);
	if (size == 0)
	{
		return ref new String();
	}

	// Allocate the buffer and do the conversion
	vector<wchar_t> buffer(size /* already includes null from pasing -1 above */);
	if (MultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, str, -1, buffer.data(), size) == 0)
	{
		return ref new String();
	}

	return ref new String(buffer.data());
}

String^ convert_to_string(unsigned char const* str)
{
	char const* newStr = reinterpret_cast<char const*>(str);
	return convert_to_string(newStr);
}

// Convert wchar_t* to a char*
char* to_cstr(wchar_t *orig)
{
    size_t origsize = wcslen(orig) + 1;
    size_t convertedChars = 0;
    char* nstring = new char[origsize];
    wcstombs_s(&convertedChars, nstring, origsize, orig, _TRUNCATE);
    
	return nstring;
}

int UnsafeNativeMethods::sqlite3_open(String^ filename, SqliteConnectionHandle^* db)
{
	auto filename_buffer = convert_to_utf8_buffer(filename);

	// Use sqlite3_open instead of sqlite3_open16 so that the default code page for stored strings is UTF-8 and not UTF-16
	sqlite3* actual_db = nullptr;
	int result = ::sqlite3_open(filename_buffer.data(), &actual_db);
	if (db)
	{
		// If they didn't give us a pointer, the caller has leaked
		*db = ref new SqliteConnectionHandle(actual_db);
	}
	return result;
}

int UnsafeNativeMethods::sqlite3_open16(String^ filename, SqliteConnectionHandle^* db)
{
	auto filename_buffer = convert_to_utf8_buffer(filename);

	sqlite3* actual_db = nullptr;
	int result = ::sqlite3_open16(filename_buffer.data(), &actual_db);
	if (db)
	{
		// If they didn't give us a pointer, the caller has leaked
		*db = ref new SqliteConnectionHandle(actual_db);
	}
	return result;
}

int UnsafeNativeMethods::sqlite3_open_v2(String^ filename, SqliteConnectionHandle^* db, int flags, String^ zVfs)
{
	auto filename_buffer = convert_to_utf8_buffer(filename);
	auto zVfs_buffer = convert_to_utf8_buffer(zVfs);

	sqlite3* actual_db = nullptr;
	int result = ::sqlite3_open_v2(
		filename_buffer.data(),
		&actual_db,
		flags,
		zVfs_buffer.size() <= 1 /* empty string */ ? nullptr : zVfs_buffer.data());
	if (db)
	{
		// If they didn't give us a pointer, the caller has leaked
		*db = ref new SqliteConnectionHandle(actual_db);
	}
	return result;
}

int UnsafeNativeMethods::sqlite3_close(SqliteConnectionHandle^ db)
{
	return::sqlite3_close(db ? db->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_busy_timeout(SqliteConnectionHandle^ db, int miliseconds)
{
	return ::sqlite3_busy_timeout(db ? db->Handle : nullptr, miliseconds);
}

int UnsafeNativeMethods::sqlite3_changes(SqliteConnectionHandle^ db)
{
	return ::sqlite3_changes(db ? db->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_prepare16(SqliteConnectionHandle^ db, String^ query, int length, SqliteStatementHandle^* statement, Platform::String^* strRemain)
{
	sqlite3_stmt* actual_statement = nullptr;
	const void* actual_tail = nullptr;
	int result = ::sqlite3_prepare16(
		db ? db->Handle : nullptr, 
		query->IsEmpty() ? L"" : query->Data(), 
		-1, 
		&actual_statement, 
		&actual_tail);
	if (statement)
	{
		// If they didn't give us a pointer, the caller has leaked
		*statement = ref new SqliteStatementHandle(actual_statement);
	}
	if (strRemain)
	{
		// If they didn't give us a pointer, the caller has leaked
		*strRemain = ref new String(reinterpret_cast<wchar_t const*>(actual_tail));
	}
	return result;
}

int UnsafeNativeMethods::sqlite3_prepare_v2(SqliteConnectionHandle^ db, String^ query, SqliteStatementHandle^* statement)
{
	sqlite3_stmt* actual_statement = nullptr;
	int result = ::sqlite3_prepare16_v2(
		db ? db->Handle : nullptr, 
		query->IsEmpty() ? L"" : query->Data(), 
		-1, 
		&actual_statement, 
		nullptr);
	if (statement)
	{
		// If they didn't give us a pointer, the caller has leaked
		*statement = ref new SqliteStatementHandle(actual_statement);
	}
	return result;
}

int UnsafeNativeMethods::sqlite3_step(SqliteStatementHandle^ statement)
{
	return ::sqlite3_step(statement ? statement->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_reset(SqliteStatementHandle^ statement)
{
	return ::sqlite3_reset(statement ? statement->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_finalize(SqliteStatementHandle^ statement)
{
	return ::sqlite3_finalize(statement ? statement->Handle : nullptr);
}

int64 UnsafeNativeMethods::sqlite3_last_insert_rowid(SqliteConnectionHandle^ db)
{
	return ::sqlite3_last_insert_rowid(db ? db->Handle : nullptr);
}

String^ UnsafeNativeMethods::sqlite3_errmsg(SqliteConnectionHandle^ db)
{
	return convert_to_string(::sqlite3_errmsg(db ? db->Handle : nullptr));
}

int UnsafeNativeMethods::sqlite3_bind_parameter_index(SqliteStatementHandle^ statement, String^ name)
{
	auto name_buffer = convert_to_utf8_buffer(name);
	return ::sqlite3_bind_parameter_index(
		statement ? statement->Handle : nullptr, 
		name_buffer.data());
}

int UnsafeNativeMethods::sqlite3_bind_null(SqliteStatementHandle^ statement, int index)
{
	return ::sqlite3_bind_null(statement ? statement->Handle : nullptr, index);
}

int UnsafeNativeMethods::sqlite3_bind_int(SqliteStatementHandle^ statement, int index, int value)
{
	return ::sqlite3_bind_int(statement ? statement->Handle : nullptr, index, value);
}

int UnsafeNativeMethods::sqlite3_bind_int64(SqliteStatementHandle^ statement, int index, int64 value)
{
	return ::sqlite3_bind_int64(statement ? statement->Handle : nullptr, index, value);
}

int UnsafeNativeMethods::sqlite3_bind_double(SqliteStatementHandle^ statement, int index, double value)
{
	return ::sqlite3_bind_double(statement ? statement->Handle : nullptr, index, value);
}

int UnsafeNativeMethods::sqlite3_bind_text(SqliteStatementHandle^ statement, int index, String^ value, int length, Object^ dummy)
{
	return ::sqlite3_bind_text(
		statement ? statement->Handle : nullptr, 
		index, 
		to_cstr(value->IsEmpty() ? L"" : value->Data()),
		length < 0 ? value->Length() * sizeof(char) : length,
		SQLITE_TRANSIENT);
}

int UnsafeNativeMethods::sqlite3_bind_text16(SqliteStatementHandle^ statement, int index, String^ value, int length)
{
	// Use transient here so that the data gets copied by sqlite
	return ::sqlite3_bind_text16(
		statement ? statement->Handle : nullptr, 
		index, 
		value->IsEmpty() ? L"" : value->Data(),
		length < 0 ? value->Length() * sizeof(wchar_t) : length,
		SQLITE_TRANSIENT);
}

int UnsafeNativeMethods::sqlite3_bind_blob(SqliteStatementHandle^ statement, int index, const Array<uint8>^ value, int length, Object^ dummy)
{
	// Use transient here so that the data gets copied by sqlite
	return ::sqlite3_bind_blob(
		statement ? statement->Handle : nullptr, 
		index, 
		value ? value->Data : nullptr, 
		length < 0 ? value->Length : length,
		SQLITE_TRANSIENT);
}

int UnsafeNativeMethods::sqlite3_column_count(SqliteStatementHandle^ statement)
{
	return ::sqlite3_column_count(statement ? statement->Handle : nullptr);
}

String^ UnsafeNativeMethods::sqlite3_column_name(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_column_name(statement ? statement->Handle : nullptr, index));
}

int UnsafeNativeMethods::sqlite3_column_type(SqliteStatementHandle^ statement, int index)
{
	return ::sqlite3_column_type(statement ? statement->Handle : nullptr, index);
}

int UnsafeNativeMethods::sqlite3_column_int(SqliteStatementHandle^ statement, int index)
{
	return ::sqlite3_column_int(statement ? statement->Handle : nullptr, index);
}

int64 UnsafeNativeMethods::sqlite3_column_int64(SqliteStatementHandle^ statement, int index)
{
	return ::sqlite3_column_int64(statement ? statement->Handle : nullptr, index);
}

double UnsafeNativeMethods::sqlite3_column_double(SqliteStatementHandle^ statement, int index)
{
	return ::sqlite3_column_double(statement ? statement->Handle : nullptr, index);
}

String^ UnsafeNativeMethods::sqlite3_column_text16(SqliteStatementHandle^ statement, int index)
{
	return ref new String(reinterpret_cast<wchar_t const*>(::sqlite3_column_text16(statement ? statement->Handle : nullptr, index)));
}

String^ UnsafeNativeMethods::sqlite3_column_text(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_column_text(statement ? statement->Handle : nullptr, index));
}

Array<uint8>^ UnsafeNativeMethods::sqlite3_column_blob(SqliteStatementHandle^ statement, int index)
{
	int count = UnsafeNativeMethods::sqlite3_column_bytes(statement, index);
	Array<uint8>^ blob = ref new Array<uint8>(count < 0 ? 0 : count);

	if (count > 0)
	{
		auto data = static_cast<uint8 const*>(::sqlite3_column_blob(statement ? statement->Handle : nullptr, index));
		std::copy(data, data + count, blob->Data);
	}

	return blob;
}

int UnsafeNativeMethods::sqlite3_column_bytes(SqliteStatementHandle^ statement, int index)
{
	return ::sqlite3_column_bytes(statement ? statement->Handle : nullptr, index);
}

void UnsafeNativeMethods::sqlite3_interrupt(SqliteConnectionHandle^ db)
{
	::sqlite3_interrupt(db ? db->Handle : nullptr);
}

SqliteStatementHandle^ UnsafeNativeMethods::sqlite3_next_stmt(SqliteConnectionHandle^ db, SqliteStatementHandle^ statement)
{
	sqlite3_stmt* newStmt = ::sqlite3_next_stmt(db ? db->Handle : nullptr, statement ? statement->Handle : nullptr);

	if (newStmt == NULL)
		return nullptr;

	return ref new SqliteStatementHandle(newStmt);
}

Platform::String^ UnsafeNativeMethods::sqlite3_value_text16(SqliteValueHandle^ value)
{
	const void* result = ::sqlite3_value_text16(value ? value->Handle : nullptr);
	return ref new String(reinterpret_cast<wchar_t const*>(result));
}

Platform::String^ UnsafeNativeMethods::sqlite3_value_text(SqliteValueHandle^ value)
{
	const unsigned char* result = ::sqlite3_value_text(value ? value->Handle : nullptr);
	return convert_to_string(result);
}

Platform::String^ UnsafeNativeMethods::sqlite3_libversion()
{
	const char* str = ::sqlite3_libversion();
	return convert_to_string(str);
}

Platform::String^ UnsafeNativeMethods::sqlite3_column_database_name16(SqliteStatementHandle^ statement, int index)
{
	const void* result = ::sqlite3_column_database_name16(statement ? statement->Handle : nullptr, index);
	return ref new String(reinterpret_cast<wchar_t const*>(result));
}

Platform::String^ UnsafeNativeMethods::sqlite3_column_origin_name16(SqliteStatementHandle^ statement, int index)
{
	const void* result = ::sqlite3_column_origin_name16(statement ? statement->Handle : nullptr, index);
	return ref new String(reinterpret_cast<wchar_t const*>(result));
}

Platform::String^ UnsafeNativeMethods::sqlite3_column_name16(SqliteStatementHandle^ statement, int index)
{
	const void* result = ::sqlite3_column_name16(statement ? statement->Handle : nullptr, index);
	return ref new String(reinterpret_cast<wchar_t const*>(result));
}

Platform::String^ UnsafeNativeMethods::sqlite3_column_table_name16(SqliteStatementHandle^ statement, int index)
{
	const void* result = ::sqlite3_column_table_name16(statement ? statement->Handle : nullptr, index);
	return ref new String(reinterpret_cast<wchar_t const*>(result));
}

void UnsafeNativeMethods::sqlite3_result_error16(SqliteContextHandle^ statement, String^ value, int index)
{
	::sqlite3_result_error16(
		statement ? statement->Handle : nullptr, 
		value->IsEmpty() ? L"" : value->Data(),
		index);
}

void UnsafeNativeMethods::sqlite3_result_text16(SqliteContextHandle^ statement, String^ value, int index)
{
	::sqlite3_result_text16(
		statement ? statement->Handle : nullptr, 
		value->IsEmpty() ? L"" : value->Data(),
		index,
		SQLITE_TRANSIENT);
}

void UnsafeNativeMethods::sqlite3_result_error(SqliteContextHandle^ statement, String^ value, int index)
{
	::sqlite3_result_error(
		statement ? statement->Handle : nullptr, 
		to_cstr(value->IsEmpty() ? L"" : value->Data()),
		index);
}

void UnsafeNativeMethods::sqlite3_result_text(SqliteContextHandle^ statement, String^ value, int index, Object^ dummy)
{
	::sqlite3_result_text(
		statement ? statement->Handle : nullptr, 
		to_cstr(value->IsEmpty() ? L"" : value->Data()),
		index,
		SQLITE_TRANSIENT);
}

int UnsafeNativeMethods::sqlite3_exec(SqliteConnectionHandle^ db, String^ query, Platform::String^* errmsg)
{
	char* actual_error = nullptr;
	int result = ::sqlite3_exec(
		db ? db->Handle : nullptr, 
		to_cstr(query->IsEmpty() ? L"" : query->Data()), 
		nullptr, 
		nullptr, 
		&actual_error);

	if (errmsg)
	{
		// If they didn't give us a pointer, the caller has leaked
		*errmsg = ref new String(reinterpret_cast<wchar_t const*>(actual_error));
	}

	if (actual_error != nullptr) 
		::sqlite3_free(actual_error);

	return result;
}

int UnsafeNativeMethods::sqlite3_bind_parameter_count(SqliteStatementHandle^ statement)
{
	return ::sqlite3_bind_parameter_count(statement ? statement->Handle : nullptr);
}

String^ UnsafeNativeMethods::sqlite3_bind_parameter_name(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_bind_parameter_name(statement ? statement->Handle : nullptr, index));
}

String^ UnsafeNativeMethods::sqlite3_column_decltype(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_column_decltype(statement ? statement->Handle : nullptr, index));
}

String^ UnsafeNativeMethods::sqlite3_column_origin_name(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_column_origin_name(statement ? statement->Handle : nullptr, index));
}

String^ UnsafeNativeMethods::sqlite3_column_database_name(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_column_database_name(statement ? statement->Handle : nullptr, index));
}

String^ UnsafeNativeMethods::sqlite3_column_table_name(SqliteStatementHandle^ statement, int index)
{
	return convert_to_string(::sqlite3_column_table_name(statement ? statement->Handle : nullptr, index));
}

int UnsafeNativeMethods::sqlite3_value_bytes(SqliteValueHandle^ value)
{
	return ::sqlite3_value_bytes(value ? value->Handle : nullptr);
}

double UnsafeNativeMethods::sqlite3_value_double(SqliteValueHandle^ value)
{
	return ::sqlite3_value_double(value ? value->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_value_int(SqliteValueHandle^ value)
{
	return ::sqlite3_value_int(value ? value->Handle : nullptr);
}

int64 UnsafeNativeMethods::sqlite3_value_int64(SqliteValueHandle^ value)
{
	return ::sqlite3_value_int64(value ? value->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_value_type(SqliteValueHandle^ value)
{
	return ::sqlite3_value_type(value ? value->Handle : nullptr);
}

Array<uint8>^ UnsafeNativeMethods::sqlite3_value_blob(SqliteValueHandle^ value)
{
	int count = UnsafeNativeMethods::sqlite3_value_bytes(value);
	Array<uint8>^ blob = ref new Array<uint8>(count < 0 ? 0 : count);

	if (count > 0)
	{
		auto data = static_cast<uint8 const*>(::sqlite3_value_blob(value ? value->Handle : nullptr));
		std::copy(data, data + count, blob->Data);
	}

	return blob;
}

void UnsafeNativeMethods::sqlite3_result_double(SqliteContextHandle^ context, double value)
{
	::sqlite3_result_double(context ? context->Handle : nullptr, value);
}

void UnsafeNativeMethods::sqlite3_result_int(SqliteContextHandle^ context, int value)
{
	::sqlite3_result_int(context ? context->Handle : nullptr, value);
}

void UnsafeNativeMethods::sqlite3_result_int64(SqliteContextHandle^ context, int64 value)
{
	::sqlite3_result_int64(context ? context->Handle : nullptr, value);
}

void UnsafeNativeMethods::sqlite3_result_null(SqliteContextHandle^ context)
{
	::sqlite3_result_null(context ? context->Handle : nullptr);
}

int UnsafeNativeMethods::sqlite3_key(SqliteConnectionHandle^ db, Platform::String^ key, int length)
{
	throw ref new NotImplementedException();
//	return ::sqlite3_key(
//		db ? db->Handle : nullptr,
//		to_cstr(key->IsEmpty() ? L"" : key->Data()),
//		length);
}

int UnsafeNativeMethods::sqlite3_rekey(SqliteConnectionHandle^ db, Platform::String^ key, int length)
{
	throw ref new NotImplementedException();
//	return ::sqlite3_rekey(
//		db ? db->Handle : nullptr, 
//		to_cstr(key->IsEmpty() ? L"" : key->Data()),
//		length);
}

void UnsafeNativeMethods::sqlite3_result_blob(SqliteContextHandle^ context, const Array<uint8>^ value, int length, Object^ dummy)
{
	::sqlite3_result_blob(
		context ? context->Handle : nullptr, 
		value ? value->Data : nullptr, 
		length < 0 ? value->Length : length, 
		nullptr);
}

int UnsafeNativeMethods::sqlite3_table_column_metadata(SqliteConnectionHandle^ db, 
													   String^ dbName, String^ tableName, String^ columnName, 
													   String^* dataType, String^* collSeq,
													   int* notNull, int* primaryKey, int* autoInc)
{
	const char* actual_dataType;
	const char* actual_collSeq;
	int result = ::sqlite3_table_column_metadata(
		db ? db->Handle : nullptr, 
		to_cstr(dbName->IsEmpty() ? L"" : dbName->Data()),
		to_cstr(tableName->IsEmpty() ? L"" : tableName->Data()),
		to_cstr(columnName->IsEmpty() ? L"" : columnName->Data()),
		&actual_dataType,
		&actual_collSeq,
		notNull,
		primaryKey,
		autoInc);

	if (dataType) *dataType = ref new String(reinterpret_cast<wchar_t const*>(actual_dataType));
	if (collSeq) *collSeq = ref new String(reinterpret_cast<wchar_t const*>(actual_collSeq));

	return result;
}

//int UnsafeNativeMethods::sqlite3_config(int option, ...)
//{
//	va_list args;
//	::sqlite3_config(option, args);
//}

void UnsafeNativeMethods::sqlite3_update_hook(SqliteConnectionHandle^ db, SqliteUpdateHookDelegate^ callback, Object^ userState)
{
	throw ref new NotImplementedException();
	//::sqlite3_update_hook(
	//	db ? db->Handle : nullptr, 
	//	callback, // how do I pass my callback?
	//	reinterpret_cast<void*>(userState)); // is this correct?
}

void UnsafeNativeMethods::sqlite3_commit_hook(SqliteConnectionHandle^ db, SqliteCommitHookDelegate^ callback, Object^ userState)
{
	throw ref new NotImplementedException();
	//::sqlite3_commit_hook(
	//	db ? db->Handle : nullptr, 
	//	callback, // how do I pass my callback?
	//	reinterpret_cast<void*>(userState)); // is this correct?
}

void UnsafeNativeMethods::sqlite3_rollback_hook(SqliteConnectionHandle^ db, SqliteRollbackHookDelegate^ callback, Object^ userState)
{
	throw ref new NotImplementedException();
	//::sqlite3_rollback_hook(
	//	db ? db->Handle : nullptr, 
	//	callback, // how do I pass my callback?
	//	reinterpret_cast<void*>(userState)); // is this correct?
}

SqliteValueHandle^ UnsafeNativeMethods::sqlite3_aggregate_context(SqliteContextHandle^ context, int nBytes)
{
	auto result = ::sqlite3_aggregate_context(context ? context->Handle : nullptr, nBytes);
	return ref new SqliteValueHandle(reinterpret_cast<sqlite3_value*>(result));
}
