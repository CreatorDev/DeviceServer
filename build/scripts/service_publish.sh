#!/usr/bin/env bash
set -e

if [ -n "$CI_BUILD_NUMBER" ]; then
  CI_BUILD_NUMBER=$(printf %06d ${CI_BUILD_NUMBER})
  SUFFIX_OPTION=" --version-suffix ci-${CI_BUILD_NUMBER}"
fi

echo "Publish services${SUFFIX_OPTION}"
for p in ${SERVICES}
do
  echo "Clean ${p}"
  rm -rf /output/publish/${p}
done

dotnet restore

for p in ${SERVICES}
do
  echo "=================================================="
  echo "*"
  echo "*  PUBLISH $p"
  echo "*"
  echo "=================================================="
  dotnet publish --configuration Release --output /output/publish/${p}${SUFFIX_OPTION} ./$p
  cp -f /app/src/$p/appsettings.json /output/publish/${p}/
done

chmod -R o+rw  /output