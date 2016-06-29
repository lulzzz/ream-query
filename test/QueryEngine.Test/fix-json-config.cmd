#!/usr/bin/env sh
rem(){ :;};rem '
@goto windows
'
# https://github.com/dotnet/cli/issues/2902
# see fix in project.json as well
cp ../../src/QueryEngine/appsettings.* .
exit
:windows
copy ..\..\src\QueryEngine\appsettings.* .
