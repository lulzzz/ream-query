#!/usr/bin/env sh
rem(){ :;};rem '
@goto windows
'
# https://github.com/dotnet/cli/issues/2902
# see fix in project.json as well
cp ../../src/QueryEngine/appsettings.* .
cp ../../src/QueryEngine/project.* bin/Debug/netcoreapp1.0/ubuntu.14.04-x64
exit
:windows
copy ..\..\src\QueryEngine\appsettings.* .
copy ..\..\src\QueryEngine\project.* bin\Debug\netcoreapp1.0\win7-x64
