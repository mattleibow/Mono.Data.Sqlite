echo off

echo Copying assemblies into directories


echo Copying System.Data PCL assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch"
copy "Output\Release\Portable\AnyCPU\System.Transactions.Portable.dll"				"NuGet\Mono.Data.Sqlite.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch\*.*"
copy "Output\Release\Portable\AnyCPU\System.Data.Portable.dll"						"NuGet\Mono.Data.Sqlite.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch\*.*"

echo Copying System.Data Windows Phone 8 assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\wp8"
copy "Output\Release\WindowsPhone8\AnyCPU\System.Transactions.Portable.dll"			"NuGet\Mono.Data.Sqlite.Portable\lib\wp8\*.*"
copy "Output\Release\WindowsPhone8\AnyCPU\System.Data.Portable.dll"					"NuGet\Mono.Data.Sqlite.Portable\lib\wp8\*.*"

echo Copying System.Data Windows Store assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\netcore"
copy "Output\Release\WindowsStore\AnyCPU\System.Transactions.Portable.dll"			"NuGet\Mono.Data.Sqlite.Portable\lib\netcore\*.*"
copy "Output\Release\WindowsStore\AnyCPU\System.Data.Portable.dll"					"NuGet\Mono.Data.Sqlite.Portable\lib\netcore\*.*"

echo Copying System.Data Silverlight 5 assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\sl5"
copy "Output\Release\Silverlight5\AnyCPU\System.Transactions.Portable.dll"			"NuGet\Mono.Data.Sqlite.Portable\lib\sl5\*.*"
copy "Output\Release\Silverlight5\AnyCPU\System.Data.Portable.dll"					"NuGet\Mono.Data.Sqlite.Portable\lib\sl5\*.*"

echo Copying System.Data Android assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\MonoAndroid"
copy "Output\Release\Android\AnyCPU\System.Transactions.Portable.dll"				"NuGet\Mono.Data.Sqlite.Portable\lib\MonoAndroid\*.*"
copy "Output\Release\Android\AnyCPU\System.Data.Portable.dll"						"NuGet\Mono.Data.Sqlite.Portable\lib\MonoAndroid\*.*"

echo Copying System.Data iOS assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\MonoTouch"
copy "Output\Release\iOS\AnyCPU\System.Transactions.Portable.dll"					"NuGet\Mono.Data.Sqlite.Portable\lib\MonoTouch\*.*"
copy "Output\Release\iOS\AnyCPU\System.Data.Portable.dll"							"NuGet\Mono.Data.Sqlite.Portable\lib\MonoTouch\*.*"

echo Copying System.Data Desktop assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\net4"
copy "Output\Release\Desktop\AnyCPU\System.Transactions.Portable.dll"				"NuGet\Mono.Data.Sqlite.Portable\lib\net4\*.*"
copy "Output\Release\Desktop\AnyCPU\System.Data.Portable.dll"						"NuGet\Mono.Data.Sqlite.Portable\lib\net4\*.*"



echo Copying Mono.Data.Sqlite PCL assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch"
copy "Output\Release\Portable\AnyCPU\Mono.Data.Sqlite.dll"							"NuGet\Mono.Data.Sqlite.Portable\lib\portable-net4+sl5+wp8+win+MonoAndroid+MonoTouch\*.*"

echo Copying Mono.Data.Sqlite Windows Phone 8 assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-arm"
copy "Output\Release\WindowsPhone8\ARM\Mono.Data.Sqlite.dll"						"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\ARM\MonoDataSqliteWrapper.dll"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\ARM\MonoDataSqliteWrapper.winmd"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-arm\*.*"
copy "Output\Release\WindowsPhone8\ARM\MonoDataSqliteWrapper.lib"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-arm\*.*"
mkdir "NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-x86"
copy "Output\Release\WindowsPhone8\x86\Mono.Data.Sqlite.dll"						"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-x86\*.*"
copy "Output\Release\WindowsPhone8\x86\MonoDataSqliteWrapper.dll"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-x86\*.*"
copy "Output\Release\WindowsPhone8\x86\MonoDataSqliteWrapper.winmd"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-x86\*.*"
copy "Output\Release\WindowsPhone8\x86\MonoDataSqliteWrapper.lib"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\wp8-x86\*.*"

echo Copying Mono.Data.Sqlite Windows Store assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-arm"
copy "Output\Release\WindowsStore\ARM\Mono.Data.Sqlite.dll"							"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\ARM\MonoDataSqliteWrapper.dll"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\ARM\MonoDataSqliteWrapper.winmd"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-arm\*.*"
copy "Output\Release\WindowsStore\ARM\MonoDataSqliteWrapper.lib"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-arm\*.*"
mkdir "NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-x86"
copy "Output\Release\WindowsStore\x86\Mono.Data.Sqlite.dll"							"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-x86\*.*"
copy "Output\Release\WindowsStore\x86\MonoDataSqliteWrapper.dll"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-x86\*.*"
copy "Output\Release\WindowsStore\x86\MonoDataSqliteWrapper.winmd"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-x86\*.*"
copy "Output\Release\WindowsStore\x86\MonoDataSqliteWrapper.lib"					"NuGet\Mono.Data.Sqlite.Portable\tools\lib\netcore-x86\*.*"

echo Copying Mono.Data.Sqlite Silverlight 5 assemblies...
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\sl5"
copy "Output\Release\Silverlight5\AnyCPU\Mono.Data.Sqlite.dll"						"NuGet\Mono.Data.Sqlite.Portable\lib\sl5\*.*"
copy "Output\Release\Silverlight5\AnyCPU\MonoDataSqliteWrapper.dll"					"NuGet\Mono.Data.Sqlite.Portable\lib\sl5\*.*"

echo Copying 3rd party libraries
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\sl5"
copy "Libraries\Community.CsharpSqlite.Silverlight.dll"								"NuGet\Mono.Data.Sqlite.Portable\lib\sl5\*.*"
mkdir "NuGet\Mono.Data.Sqlite.Portable\lib\net4"
copy "Libraries\Mono.Data.Sqlite.dll"												"NuGet\Mono.Data.Sqlite.Portable\lib\net4\*.*"

echo Copy complete. Starting NuGet packaging...

cd NuGet
rem "nuget.exe" pack System.Data.Portable/System.Data.Portable.nuspec
"nuget.exe" pack Mono.Data.Sqlite.Portable/Mono.Data.Sqlite.Portable.nuspec
cd..

echo Packaging complete