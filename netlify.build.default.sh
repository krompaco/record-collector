#!/usr/bin/env bash
set -e

pushd /tmp
wget https://dot.net/v1/dotnet-install.sh
chmod u+x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 10.0
popd

export DOTNET_ROOT=/opt/buildhome/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools

dotnet --list-sdks

npm ci && npm run prodbuild && dotnet test ./src/Krompaco.RecordCollector.Generator/Krompaco.RecordCollector.Generator.csproj --logger "console;verbosity=detailed"