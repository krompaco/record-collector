#!/usr/bin/env bash
set -e

pushd /tmp
wget https://dot.net/v1/dotnet-install.sh
chmod u+x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 10.0 --install-dir /tmp/dotnet-sdk
popd

/tmp/dotnet-sdk/dotnet --version

/tmp/dotnet-sdk/dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"