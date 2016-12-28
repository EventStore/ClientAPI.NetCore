#!/usr/bin/env bash

dotnet restore src/EventStore.ClientAPI.NetCore/project.json
dotnet restore test/EventStore.ClientAPI.NetCore.Tests/project.json
dotnet build src/EventStore.ClientAPI.NetCore/project.json
dotnet build test/EventStore.ClientAPI.NetCore.Tests/project.json
dotnet pack src/EventStore.ClientAPI.NetCore/project.json
