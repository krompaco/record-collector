#!/usr/bin/env bash
set -e

pushd /tmp
wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
chmod u+x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 8.0
popd

npm ci && npm run prodbuild && dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"