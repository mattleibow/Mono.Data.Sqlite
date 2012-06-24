Mono.Data.Sqlite
================

Mono.Data.Sqlite for restricted systems such as Silverlight/Windows Phone/WinRT

This library is still quite early in testing, but it seems to work without any issues.
I have copied the code from the Mono.Data.Sqlite, System.Data and System.Transactions directly from the Mono master. These changes were mostly member/type removals

It currently builds against the csharp-sqlite head with the 'SQLITE_ENABLE_COLUMN_METADATA' build condition and the visibility of 'static int sqlite3_table_column_metadata ( ... )' set to public in 'main_c.cs'