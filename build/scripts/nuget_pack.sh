#!/usr/bin/env bash
set -e

if [ -n "$CI_BUILD_NUMBER" ]; then
  CI_BUILD_NUMBER=$(printf %06d ${CI_BUILD_NUMBER})
  SUFFIX_OPTION=" --version-suffix ci-${CI_BUILD_NUMBER}"
fi

echo 'Create NuGet packages'
for p in ${PACKAGES}
do
  dotnet pack --configuration Release --output /output/nuget/${SUFFIX_OPTION} $p
done

chmod -R o+rw  /output