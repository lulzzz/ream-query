## .NET Core backend for the [Ream editor](https://github.com/stofte/ream-editor).

In order for the shared code to be loadable in the test context, a nuget package must be used for the shared code.
If updating the shared library, be sure to clear the nuget cache.

```
dotnet restore shared\src\ReamQuery.Shared
dotnet pack -o nuget shared\src\ReamQuery.Shared
dotnet restore query
dotnet test query\src\ReamQuery.Test
```
