echo off

echo building all projects in the solution (Release)

echo =========================================================  > output_rel.log
echo ==================== PCL ================================ >> output_rel.log

echo building any cpu (pcl)
msbuild mono.data.sqlite.sln /property:Configuration=Release;Platform="Any CPU" /verbosity:minimal > output_rel.log

echo ========================================================= >> output_rel.log
echo ==================== x86 ================================ >> output_rel.log

echo building x86 (emulators/silverlight)
msbuild mono.data.sqlite.sln /property:Configuration=Release;Platform=x86 /verbosity:minimal >> output_rel.log

echo ========================================================= >> output_rel.log
echo ==================== ARM ================================ >> output_rel.log

echo building arm (wp/ws)
msbuild mono.data.sqlite.sln /property:Configuration=Release;Platform=ARM /verbosity:minimal >> output_rel.log

echo build complete.
