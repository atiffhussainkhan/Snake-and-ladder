#!/usr/bin/env bash
# Build Snake-and-Ladder for iOS or Android from the command line.
# Usage:  ./tools/build.sh ios     | ./tools/build.sh android
set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY="/Applications/Unity/Unity-6000.3.22f1/Unity.app/Contents/MacOS/Unity"
METHOD=""
TARGET=""

case "${1:-}" in
  ios)
    METHOD="SnakeLadder.Prototype.EditorTools.DeploymentBuild.BuildIosCli"
    TARGET="iOS"
    ;;
  android)
    METHOD="SnakeLadder.Prototype.EditorTools.DeploymentBuild.BuildAndroidCli"
    TARGET="Android"
    ;;
  *)
    echo "Usage: $0 {ios|android}" >&2
    exit 1
    ;;
esac

echo "==> Building Snake-and-Ladder for $TARGET via Unity..."
"$UNITY" \
  -batchmode \
  -nographics \
  -projectPath "$PROJECT_ROOT" \
  -executeMethod "$METHOD" \
  -buildTarget "$TARGET" \
  -logFile - \
  -quit

echo "==> Done. Output is under $PROJECT_ROOT/Builds/$TARGET/"
