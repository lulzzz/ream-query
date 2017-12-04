# .NET Core backend for the [Ream editor](https://github.com/stofte/ream-editor) [![Windows build status](https://ci.appveyor.com/api/projects/status/7p2pha3iiaomihr4?svg=true)](https://ci.appveyor.com/project/stofte/ream-query) [![Linux build Status](https://travis-ci.org/stofte/ream-query.svg?branch=master)](https://travis-ci.org/stofte/ream-query)

A [Kestrel](https://github.com/aspnet/KestrelHttpServer) hosted HTTP server that performs C# query evaluation and returns output values via websockets.

Code can be standalone programs, or use [EntityFramework](https://github.com/aspnet/EntityFramework) to connect to one of

 - SQLServer
 - SQLite
 - [PostgreSQL](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/)

## Development

Latest [.NET Core 2.0](https://www.microsoft.com/net/download) is required to be installed 
in path for the following commands.

```
dotnet restore
dontet run -p tools/ReamQuery.RefDumper
dotnet test test/ReamQuery.Core.Test
dotnet test test/ReamQuery.Server.Test
```

SQL scripts for supported DB providers can be found in `sql` and must be run prior to running the tests.
Connection strings must be .NET syntax and are passed by the following environment variables:

 - `REAMQUERY_WORLDDB_SQLSERVER`
 - `REAMQUERY_WORLDDB_NPGSQL`
 - `REAMQUERY_TYPETEST_SQLSERVER`

Unset values will cause dependent tests to not run. SQLite tests depend on the included SQLite3 database `sql\world.sqlite`, so are always run.
