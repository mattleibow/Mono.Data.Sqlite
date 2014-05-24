echo off

echo Starting NuGet packaging...

cd NuGet
rem "nuget.exe" pack System.Data.Portable/System.Data.Portable.nuspec
"nuget.exe" pack Mono.Data.Sqlite.Portable/Mono.Data.Sqlite.Portable.nuspec
cd..

echo Packaging complete