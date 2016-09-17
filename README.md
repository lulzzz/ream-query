# .NET Core backend for the [Ream editor](https://github.com/stofte/ream-editor) [![Windows build status](https://ci.appveyor.com/api/projects/status/7p2pha3iiaomihr4?svg=true)](https://ci.appveyor.com/project/stofte/ream-query) [![Linux build Status](https://travis-ci.org/stofte/ream-query.svg?branch=master)](https://travis-ci.org/stofte/ream-query)

A [Kestrel](https://github.com/aspnet/KestrelHttpServer) hosted HTTP server that performs C# query evaluation and returns output values via websockets.

Code can be standalone programs, or use [EntityFramework](https://github.com/aspnet/EntityFramework) to connect to one of

 - SQLServer
 - SQLite
 - [PostgreSQL](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/)

## Development

A nuget package must be used in order for ReamQuery.Core to be loadable in a test context.
If updating it, be sure to clear the nuget cache (%USERPROFILE%\\.nuget)

```
dotnet restore core
dotnet test core/test/ReamQuery.Core.Test
dotnet pack -o nuget core/src/ReamQuery.Core
dotnet restore query
dotnet test query/test/ReamQuery.Test
```

SQL scripts for supported DB providers can be found in `sql` and must be run prior to running the tests.
Connection strings must be .NET syntax and are passed by the following environment variables:

 - `REAMQUERY_WORLDDB_SQLSERVER`
 - `REAMQUERY_WORLDDB_NPGSQL`
 - `REAMQUERY_TYPETEST_SQLSERVER`

Unset values will cause dependent tests to not run. SQLite tests depend on the included SQLite3 database `sql\world.sqlite`, so are always run.
