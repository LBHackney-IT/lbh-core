#!/bin/bash

set -e 

get_latest_production_version() {
  local package=$1
  local owner=$2

  echo "Fetching latest version for $package from GitHub Packages..." >&2

  local version=$(gh api "/orgs/$owner/packages/nuget/$package/versions" \
    --jq '[.[] | select(.name | test("^[0-9]+\\.[0-9]+\\.[0-9]+$"))] | .[0].name' 2>/dev/null || echo "0.0.0")

  if [ -z "$version" ] || [ "$version" == "null" ]; then
    version="0.0.0"
  fi

  echo "$version"
}

get_bump_type_and_distance() {
  local branch=$1
  local sha=$2
  local repo=$3
  
  local bump_type="none"
  local distance=0

  if [[ "$branch" == major/* ]]; then
    bump_type="major"
    distance=$(git rev-list --count origin/release..HEAD 2>/dev/null || echo "1")
  elif [[ "$branch" == minor/* ]]; then
    bump_type="minor"
    distance=$(git rev-list --count origin/release..HEAD 2>/dev/null || echo "1")
  elif [[ "$branch" == patch/* ]]; then
    bump_type="patch"
    distance=$(git rev-list --count origin/release..HEAD 2>/dev/null || echo "1")
  elif [[ "$branch" == "release" ]]; then
    echo "Fetching PR details for merge commit $sha..." >&2
    
    local source_branch=$(gh api "/repos/$repo/commits/$sha/pulls" \
      --jq '.[0].head.ref' 2>/dev/null || echo "")
    
    echo "Detected source branch: $source_branch" >&2

    if [[ "$source_branch" == major/* ]]; then
      bump_type="major"
    elif [[ "$source_branch" == minor/* ]]; then
      bump_type="minor"
    elif [[ "$source_branch" == patch/* ]]; then
      bump_type="patch"
    else
      echo "Notice: Source branch '$source_branch' is not major/minor/patch. Skipping publish." >&2
      bump_type="skip" 
    fi
  fi

  echo "$bump_type $distance"
}

calculate_next_version() {
  local current_version=$1
  local bump_type=$2
  
  IFS='.' read -r maj min patch <<< "$current_version"

  if [ "$bump_type" == "major" ]; then
    maj=$((maj + 1))
    min=0
    patch=0
  elif [ "$bump_type" == "minor" ]; then
    min=$((min + 1))
    patch=0
  elif [ "$bump_type" == "patch" ]; then
    patch=$((patch + 1))
  fi

  echo "${maj}.${min}.${patch}"
}

format_output_version() {
  local base_version=$1
  local branch=$2
  local distance=$3

  if [[ "$branch" == "release" ]]; then
    echo "$base_version"
  else
    local short_sha=$(git rev-parse --short HEAD)
    echo "${base_version}-${short_sha}-p${distance}"
  fi
}

main() {
  local package_name="$1"
  local owner="${GITHUB_REPOSITORY%/*}"
  local branch_name="${GITHUB_REF#refs/heads/}"

  echo "--- Starting version calculation for $package_name ---"

  local latest_version=$(get_latest_production_version "$package_name" "$owner")
  echo "Base production version found: $latest_version"

  read -r bump_type distance <<< "$(get_bump_type_and_distance "$branch_name" "$GITHUB_SHA" "$GITHUB_REPOSITORY")"
  
  if [ "$bump_type" == "skip" ]; then
    echo "should_publish=false" >> "$GITHUB_OUTPUT"
    echo "Aborting version calculation."
    exit 0
  fi

  echo "Bump type resolved to: $bump_type (Distance: $distance)"

  local new_base_version=$(calculate_next_version "$latest_version" "$bump_type")
  echo "New base semantic version: $new_base_version"

  local final_version=$(format_output_version "$new_base_version" "$branch_name" "$distance")
  echo "Calculated Version: $final_version"

  # mechanism for preserving variables to next Github Actions step
  echo "version=$final_version" >> "$GITHUB_OUTPUT"
  echo "should_publish=true" >> "$GITHUB_OUTPUT"
}

PACKAGE_NAME="$1"

main "$PACKAGE_NAME"
