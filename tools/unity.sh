#!/usr/bin/env bash
# Local Unity tasks that need the editor license on this machine.
# Close the project in the Unity editor first: batch mode cannot open it twice.
#   tools/unity.sh test      EditMode tests, summary in the terminal
#   tools/unity.sh apk       Android APK into Builds/Android/FivesGame.apk
#   tools/unity.sh webgl     WebGL build into Builds/WebGL
#   tools/unity.sh publish   push Builds/WebGL to the gh-pages branch (GitHub Pages)
set -euo pipefail

ROOT=$(cd "$(dirname "$0")/.." && pwd)
VERSION=$(sed -n 's/^m_EditorVersion: //p' "$ROOT/ProjectSettings/ProjectVersion.txt")
UNITY=${UNITY:-/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/MacOS/Unity}
WEBGL="$ROOT/Builds/WebGL"

unity() {
  "$UNITY" -batchmode -nographics -projectPath "$ROOT" -logFile "$ROOT/Logs/batch.log" "$@"
}

case "${1:-}" in
  test)
    results="$ROOT/Logs/editmode-results.xml"
    code=0
    unity -runTests -testPlatform EditMode -testResults "$results" || code=$?
    grep -o '<test-run [^>]*' "$results" | grep -oE '(total|passed|failed|skipped)="[0-9]+"' | tr '\n' ' '
    echo
    grep -B1 -A3 'result="Failed"' "$results" | grep -oE '(fullname="[^"]+"|<message>.*)' | head -40 || true
    exit $code
    ;;
  apk)
    unity -quit -buildTarget Android -executeMethod Fives.Editor.AndroidBuild.BuildRelease
    echo "Built $ROOT/Builds/Android/FivesGame.apk"
    ;;
  webgl)
    unity -quit -buildTarget WebGL -executeMethod Fives.Editor.WebGLBuild.Build
    echo "Built $WEBGL"
    ;;
  publish)
    [ -f "$WEBGL/index.html" ] || { echo "No build in $WEBGL, run: tools/unity.sh webgl"; exit 1; }
    remote=$(git -C "$ROOT" remote get-url origin)
    commit=$(git -C "$ROOT" rev-parse --short HEAD)
    site=$(mktemp -d)
    cp -R "$WEBGL/." "$site"
    touch "$site/.nojekyll"
    # One orphan commit per deploy: the branch holds only the current build, not a history of binaries.
    git -C "$site" init -q -b gh-pages
    git -C "$site" add -A
    git -C "$site" commit -q -m "Deploy WebGL build from $commit"
    git -C "$site" push -q -f "$remote" gh-pages
    rm -rf "$site"
    echo "Published $commit to gh-pages"
    ;;
  *)
    sed -n '4,7p' "$0"
    exit 1
    ;;
esac
