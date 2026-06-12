#!/bin/bash

set -e 

PACKAGE_NAME="$1"
BASE_VERSION="$2"
OWNER="${GITHUB_REPOSITORY%/*}"

echo "Detected owner: *$OWNER*, from: *$GITHUB_REPOSITORY*"

if [ -z "$PACKAGE_NAME" ] || [ -z "$BASE_VERSION" ]; then
  echo "Notice: Package name or base version is unset. Skipping preview cleanup."
  exit 0
fi

echo "Searching for preview versions of $PACKAGE_NAME tied to v$BASE_VERSION..."

ALL_VERSIONS=$(gh api "/orgs/$OWNER/packages/nuget/$PACKAGE_NAME/versions")
echo "All versions:"
echo "$ALL_VERSIONS"
echo "------AV-----"

echo "Now JQ!"
echo " ####JQ ARR #### "
VR_JQ=$(gh api "/orgs/$OWNER/packages/nuget/$PACKAGE_NAME/versions" --jq --arg prefix "$BASE_VERSION-" '.[]')
echo "$VR_JQ"
echo "##############"
echo "### VR FILTER ###"
VR_FILTER=$(gh api "/orgs/$OWNER/packages/nuget/$PACKAGE_NAME/versions" --jq --arg prefix "$BASE_VERSION-" '.[] | select(.name | startswith($prefix))')
echo "$VR_FILTER"
echo "### Done ###"


# 2>/dev/null || echo ""
VERSION_IDS=$(gh api "/orgs/$OWNER/packages/nuget/$PACKAGE_NAME/versions" \
  --jq --arg prefix "$BASE_VERSION-" '.[] | select(.name | startswith($prefix)) | .id')

if [ -z "$VERSION_IDS" ]; then
  echo "No preview packages found for v$BASE_VERSION to clean up."
  exit 0
fi

for ID in $VERSION_IDS; do
  echo "Deleting preview package ID: $ID..."
  gh api -X DELETE "/orgs/$OWNER/packages/nuget/$PACKAGE_NAME/versions/$ID"
done

echo "Successfully cleaned up preview packages for $PACKAGE_NAME v$BASE_VERSION!"
