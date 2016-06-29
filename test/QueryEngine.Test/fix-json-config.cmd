rem https://github.com/dotnet/cli/issues/2902
rem see fix in project.json for test project as well 
copy ..\..\src\QueryEngine\appsettings.json appsettings.json
copy ..\..\src\QueryEngine\appsettings.test.json appsettings.test.json
