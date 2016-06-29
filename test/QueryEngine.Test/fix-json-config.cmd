rem(){ :;};rem '
@goto b
# https://github.com/dotnet/cli/issues/2902
# see fix in project.json for test project as well
';cp ../../src/QueryEngine/appsettings.* *;exit
:b
copy ..\..\src\QueryEngine\appsettings.* .
