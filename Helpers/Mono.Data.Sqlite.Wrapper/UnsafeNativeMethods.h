#pragma once

#include <sqlite3.h>

namespace MonoDataSqliteWrapper
			{
				/*
				Utility class for wrapping sqlite3 "handles".
				*/
				public ref class SqliteConnectionHandle sealed
				{
				internal:
					SqliteConnectionHandle(sqlite3* db) : _handle(db)
					{
					}

					property sqlite3* Handle
					{ 
						sqlite3* get()
						{
							return _handle;
						}
					}

				private:
					sqlite3* _handle;
				};

				/*
				Utility class for wrapping sqlite3 "handles".
				*/
				public ref class SqliteValueHandle sealed
				{
				internal:
					SqliteValueHandle(sqlite3_value* db) : _handle(db)
					{
					}

					property sqlite3_value* Handle
					{ 
						sqlite3_value* get()
						{
							return _handle;
						}
					}

				private:
					sqlite3_value* _handle;
				};

				/*
				Utility class for wrapping sqlite3_stmt "handles".
				*/
				public ref class SqliteStatementHandle sealed
				{
				internal:
					SqliteStatementHandle(sqlite3_stmt* statement) : _handle(statement)
					{
					}

					property sqlite3_stmt* Handle
					{ 
						sqlite3_stmt* get()
						{
							return _handle;
						}
					}

				private:
					sqlite3_stmt* _handle;
				};

				/*
				Utility class for wrapping sqlite3_context "handles".
				*/
				public ref class SqliteContextHandle sealed
				{
				internal:
					SqliteContextHandle(sqlite3_context* context) : _handle(context)
					{
					}

					property sqlite3_context* Handle
					{ 
						sqlite3_context* get()
						{
							return _handle;
						}
					}

				private:
					sqlite3_context* _handle;
				};

				//public delegate void SQLiteCallback(SqliteContextHandle^ context, int nArgs, const Platform::Array<SqliteValueHandle^>^ args);

				public delegate void SqliteUpdateHookDelegate(
					Platform::Object^ userState, 
					int operationFlag, 
					Platform::String^ dbName, 
					Platform::String^ tableName, 
					int64 rowid);

				public delegate int SqliteCommitHookDelegate(Platform::Object^ userState);

				public delegate void SqliteRollbackHookDelegate(Platform::Object^ userState);

				/*
				This class is simply a C++/CX wrapper around sqlite3 exports that sqlite.net depends on.
				Consult the sqlite documentation on what they do.
				*/
				public ref class UnsafeNativeMethods sealed
				{
				public:
					static int sqlite3_open(Platform::String^ filename, SqliteConnectionHandle^* db);
					static int sqlite3_open16(Platform::String^ filename, SqliteConnectionHandle^* db);
					static int sqlite3_open_v2(Platform::String^ filename, SqliteConnectionHandle^* db, int flags, Platform::String^ zVfs);
					static int sqlite3_close(SqliteConnectionHandle^ db);
					static int sqlite3_busy_timeout(SqliteConnectionHandle^ db, int miliseconds);
					static int sqlite3_changes(SqliteConnectionHandle^ db);
					static int sqlite3_prepare16(SqliteConnectionHandle^ db, Platform::String^ query, int length, SqliteStatementHandle^* statement, Platform::String^* strRemain);
					static int sqlite3_prepare_v2(SqliteConnectionHandle^ db, Platform::String^ query, SqliteStatementHandle^* statement);
					static int sqlite3_step(SqliteStatementHandle^ statement);
					static int sqlite3_reset(SqliteStatementHandle^ statement);
					static int sqlite3_finalize(SqliteStatementHandle^ statement);
					static int64 sqlite3_last_insert_rowid(SqliteConnectionHandle^ db);
					static Platform::String^ sqlite3_errmsg(SqliteConnectionHandle^ db);
					static int sqlite3_bind_parameter_index(SqliteStatementHandle^ statement, Platform::String^ name);
					static int sqlite3_bind_null(SqliteStatementHandle^ statement, int index);
					static int sqlite3_bind_int(SqliteStatementHandle^ statement, int index, int value);
					static int sqlite3_bind_int64(SqliteStatementHandle^ statement, int index, int64 value);
					static int sqlite3_bind_double(SqliteStatementHandle^ statement, int index, double value);
					static int sqlite3_bind_text(SqliteStatementHandle^ statement, int index, Platform::String^ value, int length, Platform::Object^ dummy);
					static int sqlite3_bind_text16(SqliteStatementHandle^ statement, int index, Platform::String^ value, int length);
					static int sqlite3_bind_blob(SqliteStatementHandle^ statement, int index, const Platform::Array<uint8>^ value, int length, Platform::Object^ dummy);	
					static int sqlite3_column_count(SqliteStatementHandle^rstatement);
					static Platform::String^ sqlite3_column_name(SqliteStatementHandle^ statement, int index);
					static int sqlite3_column_type(SqliteStatementHandle^ statement, int index);
					static int sqlite3_column_int(SqliteStatementHandle^ statement, int index);
					static int64 sqlite3_column_int64(SqliteStatementHandle^ statement, int index);
					static double sqlite3_column_double(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_text16(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_text(SqliteStatementHandle^ statement, int index);
					static Platform::Array<uint8>^ sqlite3_column_blob(SqliteStatementHandle^, int index);
					static int sqlite3_column_bytes(SqliteStatementHandle^ statement, int index);
					static void sqlite3_interrupt(SqliteConnectionHandle^ db);
					static SqliteStatementHandle^ sqlite3_next_stmt(SqliteConnectionHandle^ db, SqliteStatementHandle^ statement);
					static Platform::String^ sqlite3_value_text16(SqliteValueHandle^ value);
					static Platform::String^ sqlite3_value_text(SqliteValueHandle^ value);
					static Platform::String^ sqlite3_libversion();
					static Platform::String^ sqlite3_column_database_name16(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_origin_name16(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_name16(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_table_name16(SqliteStatementHandle^ statement, int index);
					static void sqlite3_result_error16(SqliteContextHandle^ statement, Platform::String^ value, int index);
					static void sqlite3_result_text16(SqliteContextHandle^ statement, Platform::String^ value, int index);
					static void sqlite3_result_error(SqliteContextHandle^ statement, Platform::String^ value, int index);
					static void sqlite3_result_text(SqliteContextHandle^ statement, Platform::String^ value, int index, Platform::Object^ dummy);
					static int sqlite3_exec(SqliteConnectionHandle^ db, Platform::String^ query, Platform::String^* errmsg);
					static int sqlite3_bind_parameter_count(SqliteStatementHandle^ statement);
					static Platform::String^ sqlite3_bind_parameter_name(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_decltype(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_origin_name(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_database_name(SqliteStatementHandle^ statement, int index);
					static Platform::String^ sqlite3_column_table_name(SqliteStatementHandle^ statement, int index);
					static int sqlite3_value_bytes(SqliteValueHandle^ value);
					static double sqlite3_value_double(SqliteValueHandle^ value);
					static int sqlite3_value_int(SqliteValueHandle^ value);
					static int64 sqlite3_value_int64(SqliteValueHandle^ value);
					static int sqlite3_value_type(SqliteValueHandle^ value);
					static Platform::Array<uint8>^ sqlite3_value_blob(SqliteValueHandle^ value);
					static void sqlite3_result_double(SqliteContextHandle^ statement, double value);
					static void sqlite3_result_int(SqliteContextHandle^ statement, int value);
					static void sqlite3_result_int64(SqliteContextHandle^ statement, int64 value);
					static void sqlite3_result_null(SqliteContextHandle^ statement);
					static void sqlite3_result_blob(SqliteContextHandle^ context, const Platform::Array<uint8>^ value, int length, Platform::Object^ dummy);
					static int sqlite3_table_column_metadata(SqliteConnectionHandle^ db,
						Platform::String^ dbName, Platform::String^ tableName, Platform::String^ columnName, 
						Platform::String^* dataType, Platform::String^* collSeq, 
						int* notNull, int* primaryKey, int* autoInc);
					static int sqlite3_key(SqliteConnectionHandle^ db, Platform::String^ key, int length);
					static int sqlite3_rekey(SqliteConnectionHandle^ db, Platform::String^ key, int length);
					//static int sqlite3_config(int option, ...);
					static void sqlite3_update_hook(SqliteConnectionHandle^ db, SqliteUpdateHookDelegate^ callback, Platform::Object^ userState);
					static void sqlite3_commit_hook(SqliteConnectionHandle^ db, SqliteCommitHookDelegate^ callback, Platform::Object^ userState);
					static void sqlite3_rollback_hook(SqliteConnectionHandle^ db, SqliteRollbackHookDelegate^ callback, Platform::Object^ userState);
					static SqliteValueHandle^ sqlite3_aggregate_context(SqliteContextHandle^ context, int nBytes);
				};
			}