#!/usr/bin/env bash
# Local Unity tasks that need the editor license on this machine.
# Close the project in the Unity editor first: batch mode cannot open it twice.
#   tools/unity.sh test                          EditMode tests, summary in the terminal
#   tools/unity.sh apk                           Android APK into Builds/Android/FivesGame.apk, with its content
#   tools/unity.sh webgl                         WebGL build into Builds/WebGL, with its content
#   tools/unity.sh content-build android|webgl   theme bundles and catalog into ServerData/<platform>, no player
#   tools/unity.sh content android|webgl         content update for the released player: changed bundles only
#   tools/unity.sh publish                       the WebGL game to GitHub Pages; the content stays as it is
#   tools/unity.sh publish-content               ServerData/<platform> to GitHub Pages; the game stays as it is
set -euo pipefail

ROOT=$(cd "$(dirname "$0")/.." && pwd)
VERSION=$(sed -n 's/^m_EditorVersion: //p' "$ROOT/ProjectSettings/ProjectVersion.txt")
UNITY=${UNITY:-/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/MacOS/Unity}
WEBGL="$ROOT/Builds/WebGL"

unity() {
  "$UNITY" -batchmode -nographics -projectPath "$ROOT" -logFile "$ROOT/Logs/batch.log" "$@"
}

platform() {
  case "${1:-}" in
    android) echo Android ;;
    webgl) echo WebGL ;;
    *) echo "Expected android or webgl" >&2; exit 1 ;;
  esac
}

# The site as GitHub Pages serves it now, in a new directory; empty when gh-pages does not exist yet. The game sits at
# the root and the content under content/, so each publish replaces its own part and keeps the other.
site_checkout() {
  local remote=$1 site found=0
  site=$(mktemp -d)
  git ls-remote --exit-code --heads "$remote" gh-pages >/dev/null || found=$?
  if [ "$found" -eq 0 ]; then
    # A failed clone stops here: pushing without it would drop the part that is not being published.
    git clone -q --depth 1 --branch gh-pages "$remote" "$site"
    rm -rf "$site/.git"
  elif [ "$found" -ne 2 ]; then # 2: no such branch yet; anything else: the remote was not reached
    echo "Cannot read gh-pages from $remote" >&2; exit 1
  fi
  echo "$site"
}

# One orphan commit per deploy: the branch holds only the current site, not a history of binaries.
site_push() {
  local remote=$1 site=$2 message=$3
  touch "$site/.nojekyll"
  git -C "$site" init -q -b gh-pages
  # --force: a global excludes file (for example the Unity template ignoring Build/) must not drop site files.
  git -C "$site" add --all --force
  git -C "$site" commit -q -m "$message"
  git -C "$site" push -q -f "$remote" gh-pages
  rm -rf "$site"
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
    echo "Built $ROOT/Builds/Android/FivesGame.apk; its content: tools/unity.sh publish-content"
    ;;
  webgl)
    unity -quit -buildTarget WebGL -executeMethod Fives.Editor.WebGLBuild.Build
    echo "Built $WEBGL; publish it with its content: tools/unity.sh publish && tools/unity.sh publish-content"
    ;;
  content-build)
    target=$(platform "${2:-}")
    unity -quit -buildTarget "$target" -executeMethod Fives.Editor.ContentBuild.Build
    echo "Built $ROOT/ServerData/$target; publish it with: tools/unity.sh publish-content"
    ;;
  content)
    target=$(platform "${2:-}")
    unity -quit -buildTarget "$target" -executeMethod Fives.Editor.ContentBuild.Update
    echo "Built $ROOT/ServerData/$target; publish it with: tools/unity.sh publish-content"
    ;;
  publish)
    [ -f "$WEBGL/index.html" ] || { echo "No build in $WEBGL, run: tools/unity.sh webgl"; exit 1; }
    remote=$(git -C "$ROOT" remote get-url origin)
    commit=$(git -C "$ROOT" rev-parse --short HEAD)
    site=$(site_checkout "$remote")
    find "$site" -mindepth 1 -maxdepth 1 ! -name content -exec rm -rf {} +
    cp -R "$WEBGL/." "$site"
    site_push "$remote" "$site" "Deploy the game from $commit"
    echo "Published the game from $commit to gh-pages"
    ;;
  publish-content)
    ls "$ROOT"/ServerData/*/ >/dev/null 2>&1 || { echo "No content in ServerData, run: tools/unity.sh content-build"; exit 1; }
    remote=$(git -C "$ROOT" remote get-url origin)
    commit=$(git -C "$ROOT" rev-parse --short HEAD)
    site=$(site_checkout "$remote")
    # Content is only added: installed players keep loading the catalogs and bundles of their own build.
    for target in Android WebGL; do
      if [ -d "$ROOT/ServerData/$target" ]; then
        mkdir -p "$site/content/$target"
        cp -R "$ROOT/ServerData/$target/." "$site/content/$target"
      fi
    done
    site_push "$remote" "$site" "Deploy content from $commit"
    echo "Published the content from $commit to gh-pages"
    ;;
  *)
    sed -n '4,10p' "$0"
    exit 1
    ;;
esac
