#!/usr/bin/env bash

dotnet restore src/EventStore.ClientAPI.NetCore/EventStore.ClientAPI.NetCore.csproj
dotnet restore test/EventStore.ClientAPI.NetCore.Tests/EventStore.ClientAPI.NetCore.Tests.csproj
dotnet build src/EventStore.ClientAPI.NetCore/EventStore.ClientAPI.NetCore.csproj
dotnet build test/EventStore.ClientAPI.NetCore.Tests/EventStore.ClientAPI.NetCore.Tests.csproj
dotnet pack src/EventStore.ClientAPI.NetCore/EventStore.ClientAPI.NetCore.csproj