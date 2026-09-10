#!/usr/bin/env bash
set -euo pipefail

mkdir -p test-results
APK_PATH="$(find src/GymTrackerMobile/bin/Debug/net10.0-android -maxdepth 1 -name '*.apk' -print -quit)"
if [[ -z "$APK_PATH" ]]; then
  echo "No Android APK was found under src/GymTrackerMobile/bin/Debug/net10.0-android" >&2
  exit 1
fi

appium --base-path /wd/hub > test-results/appium.log 2>&1 &
APPIUM_PID=$!
trap 'kill "$APPIUM_PID" 2>/dev/null || true' EXIT

sleep 5
APPIUM_SERVER_URL=http://127.0.0.1:4723/wd/hub \
ANDROID_DEVICE_NAME=GymTrackerApi35 \
GYMTRACKER_APK_PATH="$GITHUB_WORKSPACE/$APK_PATH" \
dotnet test tests/GymTrackerMobile.UI.AutomationTests/GymTrackerMobile.UI.AutomationTests.csproj \
  --configuration Release \
  --no-restore \
  --logger "trx;LogFileName=android-ui.trx" \
  --results-directory test-results
