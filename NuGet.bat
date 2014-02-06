echo Copying assemblies into directories


echo Copying System.Data PCL assemblies...
copy "Output\Release\Portable\AnyCPU\System.Transactions.dll"				"NuGet\System.Data.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch\*.*"
copy "Output\Release\Portable\AnyCPU\System.Data.dll"					"NuGet\System.Data.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch\*.*"

echo Copying System.Data Windows Phone 8 assemblies...
copy "Output\Release\WindowsPhone8\AnyCPU\System.Transactions.dll"			"NuGet\System.Data.Portable\lib\wp8\*.*"
copy "Output\Release\WindowsPhone8\AnyCPU\System.Data.dll"				"NuGet\System.Data.Portable\lib\wp8\*.*"

echo Copying System.Data Windows Store assemblies...
copy "Output\Release\WindowsStore\AnyCPU\System.Transactions.dll"			"NuGet\System.Data.Portable\lib\netcore\*.*"
copy "Output\Release\WindowsStore\AnyCPU\System.Data.dll"				"NuGet\System.Data.Portable\lib\netcore\*.*"

echo Copying System.Data Silverlight 5 assemblies...
copy "Output\Release\Silverlight5\AnyCPU\System.Transactions.dll"			"NuGet\System.Data.Portable\lib\sl50\*.*"
copy "Output\Release\Silverlight5\AnyCPU\System.Data.dll"				"NuGet\System.Data.Portable\lib\sl50\*.*"



echo Copying Mono.Data.Sqlite PCL assemblies...
copy "Output\Release\Portable\AnyCPU\Mono.Data.Sqlite.dll"				"NuGet\Mono.Data.Sqlite.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch\*.*"

echo Copying Mono.Data.Sqlite Windows Phone 8 assemblies...
copy "Output\Release\WindowsPhone8\ARM\Mono.Data.Sqlite.dll"				"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\ARM\MonoDataSqliteWrapper.dll"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\ARM\MonoDataSqliteWrapper.winmd"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\ARM\MonoDataSqliteWrapper.lib"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\x86\Mono.Data.Sqlite.dll"				"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-x86\*.*"
copy "Output\Release\WindowsPhone8\x86\MonoDataSqliteWrapper.dll"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-x86\*.*"
copy "Output\Release\WindowsPhone8\x86\MonoDataSqliteWrapper.winmd"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-x86\*.*"
copy "Output\Release\WindowsPhone8\x86\MonoDataSqliteWrapper.lib"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\wp8-x86\*.*"

echo Copying Mono.Data.Sqlite Windows Store assemblies...
copy "Output\Release\WindowsStore\ARM\Mono.Data.Sqlite.dll"				"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\ARM\MonoDataSqliteWrapper.dll"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\ARM\MonoDataSqliteWrapper.winmd"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\ARM\MonoDataSqliteWrapper.lib"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\x86\Mono.Data.Sqlite.dll"				"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-x86\*.*"
copy "Output\Release\WindowsStore\x86\MonoDataSqliteWrapper.dll"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-x86\*.*"
copy "Output\Release\WindowsStore\x86\MonoDataSqliteWrapper.winmd"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-x86\*.*"
copy "Output\Release\WindowsStore\x86\MonoDataSqliteWrapper.lib"			"NuGet\Mono.Data.Sqlite.Portable\build\lib\netcore-x86\*.*"

echo Copying Mono.Data.Sqlite Silverlight 5 assemblies...
copy "Output\Release\Silverlight5\AnyCPU\Mono.Data.Sqlite.dll"				"NuGet\Mono.Data.Sqlite.Portable\lib\sl50\*.*"
copy "Output\Release\Silverlight5\AnyCPU\MonoDataSqliteWrapper.dll"			"NuGet\Mono.Data.Sqlite.Portable\lib\sl50\*.*"
copy "Output\Release\Silverlight5\AnyCPU\Community.CsharpSqlite.Silverlight.dll"	"NuGet\Mono.Data.Sqlite.Portable\lib\sl50\*.*"
