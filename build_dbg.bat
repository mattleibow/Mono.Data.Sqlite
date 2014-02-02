rem build all projects in the solution (DEBUG)

echo ========================================================= >> output_dbg.log
echo ==================== PCL ================================ >> output_dbg.log

rem any (pcl)
msbuild mono.data.sqlite.sln /property:Configuration=Debug;Platform="Any CPU" /verbosity:minimal > output_dbg.log

echo ========================================================= >> output_dbg.log
echo ==================== x86 ================================ >> output_dbg.log

rem x86 (emulators/silverlight)
msbuild mono.data.sqlite.sln /property:Configuration=Debug;Platform=x86 /verbosity:minimal >> output_dbg.log

echo ========================================================= >> output_dbg.log
echo ==================== ARM ================================ >> output_dbg.log

rem arm (wp/ws)
msbuild mono.data.sqlite.sln /property:Configuration=Debug;Platform=ARM /verbosity:minimal >> output_dbg.log