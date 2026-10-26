#!/bin/bash

set -euo pipefail

# empty default so we get passed `set -u` when values are missing & would get a custom error
project_root=${1:-}
pack_project=${2:-}
package_name=${3:-}

if [ -z "$project_root" ] || [ -z "$pack_project" ] || [ -z "$package_name" ]; then
  echo "Usage: sonar-scan.sh <project_root> <pack_project> <package_name>" >&2
  exit 1
fi

if [ -z "${SONAR_TOKEN:-}" ]; then
  echo "SONAR_TOKEN is required" >&2
  exit 1
fi

workspace="${GITHUB_WORKSPACE:-$(pwd)}"

# stick to dotnet8 regardless of what runner is on.
# can be bumped to v10 once dotnet10 migrations are completed
cat > global.json <<'EOF'
{
  "sdk": {
    "version": "8.0.100",
    "rollForward": "latestFeature"
  }
}
EOF

# convert code tests coverage file paths from within the container to ones on github actions runner
coverage_args=()
if [ -d coverage ] && find coverage -name coverage.opencover.xml -print -quit | grep -q .; then
  find coverage -name coverage.opencover.xml -exec sed -i "s|/app/|${workspace}/|g" {} +
  coverage_args+=(/d:sonar.cs.opencover.reportsPaths="coverage/**/coverage.opencover.xml")
fi

dotnet tool install --global dotnet-sonarscanner --version 12.0.0
export PATH="$PATH:$HOME/.dotnet/tools"

args=(
  /k:"LBHackney-IT_lbh-core_${package_name}"
  /o:"lbhackney-it"
  /n:"${package_name}"
  /d:sonar.host.url="https://sonarcloud.io"
  /d:sonar.token="${SONAR_TOKEN}"
  /d:sonar.dotnet.excludeTestProjects=true
  /d:sonar.exclusions="**/*.js,**/*.ts,**/*.css"
  /d:sonar.sourceEncoding="UTF-8"
)
if [ ${#coverage_args[@]} -gt 0 ]; then
  args+=("${coverage_args[@]}")
fi

# sonar recommends using dotnet sonar scanner. it needs to be wrapped around dotnet build
dotnet sonarscanner begin "${args[@]}"
dotnet build "${project_root}/${pack_project}/${pack_project}.csproj"
dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
