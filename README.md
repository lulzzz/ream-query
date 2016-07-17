## .NET Core backend for the [Ream editor](https://github.com/stofte/ream-editor) [![Windows build status](https://ci.appveyor.com/api/projects/status/7p2pha3iiaomihr4?svg=true)](https://ci.appveyor.com/project/stofte/ream-query) [![Linux build Status](https://travis-ci.org/stofte/ream-query.svg?branch=master)](https://travis-ci.org/stofte/ream-query)

A nuget package must be used in order for the shared code to be loadable in a test context.
If updating shared, be sure to clear the nuget cache (%USERPROFILE%\\.nuget)

```
dotnet restore shared
dotnet test shared\test\ReamQuery.Shared.Test
dotnet pack -o nuget shared\src\ReamQuery.Shared
dotnet restore query
dotnet test query\test\ReamQuery.Test
```

SQL scripts for supported DB providers can be found in `sql` and must be run prior to running the tests.
Connection strings must be .NET syntax and are passed by the following environment variables:

 - `REAMQUERY_WORLDDB_SQLSERVER`
 - `REAMQUERY_TYPETEST_SQLSERVER`
 - `REAMQUERY_WORLDDB_NPGSQL`

Unset values will cause dependent tests to not run.
