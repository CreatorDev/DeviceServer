#!/usr/bin/env bash
set -e

# defaults
if [ -z "$NUGETSERVER" ]; then
  NUGETSERVER=10.100.125.41
fi
if [ -z "$NUGETKEY" ]; then
  NUGETKEY=a5fe9c31-4fcc-48ce-8c6b-9be47dd5251c
fi

echo "Push NuGet packages to ${NUGETSERVER}"
for p in *.nupkg
do
  # assume symbol server not configured
  if [[ $p != *.symbols.nupkg ]]; then
    nuget push $p -Source http://${NUGETSERVER}/api/odata -ApiKey ${NUGETKEY} # -Verbosity detailed
  fi
done