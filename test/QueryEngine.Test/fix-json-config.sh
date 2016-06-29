#!/usr/bin/env sh
# https://github.com/dotnet/cli/issues/2902
# see fix in project.json for test project as well 
cp ../../src/QueryEngine/appsettings.json appsettings.json
cp ../../src/QueryEngine/appsettings.test.json appsettings.test.json
