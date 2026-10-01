# Deployment Guide

Build Snake-and-Ladder for the iOS App Store and Google Play Store.

## Prerequisites

### Unity Editor + Build Modules

The Unity Editor already on this machine (`/Applications/Unity/Unity-6000.3.22f1/`) needs the **iOS Build Support** and **Android Build Support** modules. Install them via Unity Hub:

```bash
open "/Applications/Unity Hub.app"
# → Installs → your 6000.3.22f1 editor → Add modules → tick iOS Build Support + Android Build Support
```

Or via the Hub CLI:

```bash
/Applications/Unity\ Hub.app/Contents/MacOS/Unity\ Hub --headless install-modules --version 6000.3.22f1 --module ios,android
```

### iOS toolchain (App Store only)

Install full Xcode (not just CommandLineTools), then install the command-line tools:

```bash
xcode-select --install        # if not already
sudo xcode-select -s /Applications/Xcode.app/Contents/Developer
xcodebuild -version            # confirm
```

Sign up for an Apple Developer account at https://developer.apple.com ($99 USD/year).

### Android toolchain (Play Store only)

```bash
brew install --cask android-commandlinetools
# Accept SDK licenses:
yes | sdkmanager --licenses
# Install the SDK platforms Unity will target:
sdkmanager "platform-tools" "platforms;android-34" "build-tools;34.0.0"
# Set environment variable so Unity finds the SDK:
export ANDROID_SDK_ROOT="$HOME/Library/Android/sdk"
```

Sign up for a Google Play Console account at https://play.google.com/console ($25 USD one-time).

## Build

The build is a single command per platform:

```bash
cd /Users/mac/Documents/Codex/Projects/Snake-and-ladder
./tools/build.sh ios        # → Builds/iOS/  (Xcode project)
./tools/build.sh android    # → Builds/Android/  (APK)
```

Or from the Unity editor: **Tools → Snake-Ladder → Build iOS Xcode Project** / **Build Android APK**.

The build script first calls `DeploymentBuild.ConfigureProjectSettings()` (sets bundle id, version, IL2CPP, ARM64, min SDK, etc.) and then triggers the actual player build. A fresh checkout can therefore go straight from clone → binary in one command.

## Submitting

### iOS → App Store

```bash
cd Builds/iOS
open SnakeLadderPrototype.xcworkspace    # the .xcworkspace, not the .xcodeproj
```

In Xcode:

1. Select the **SnakeLadderPrototype** target → **Signing & Capabilities** → set your Team.
2. Edit the bundle identifier if you want a different reverse-DNS than the default `com.yourstudio.snakesandladders`.
3. **Product → Archive** (release build).
4. In Organizer → **Distribute App → App Store Connect → Upload**.
5. Log in to https://appstoreconnect.apple.com → My Apps → create a new app → fill in the metadata → the build you uploaded appears within a few minutes → submit for review.

Typical Apple review time: 24–48 h.

### Android → Play Store

```bash
cd Builds/Android
# You now have SnakeLadderPrototype.apk (unsigned). Generate an upload key + sign it:
keytool -genkey -v -keystore snakes-ladders-upload.keystore -alias upload -keyalg RSA -keysize 2048 -validity 10000
# Configure Unity to use this keystore under Edit > Project Settings > Player > Publishing Settings
# (or via PlayerSettings.Android.androidKeyStoreAliasName etc. in DeploymentBuild.ApplyAndroidSettings)
# Re-build → produces a signed APK / AAB.
```

Google Play now requires **AAB (Android App Bundle)** format for new apps:

```bash
# Open Unity, switch the build format under Build Settings → Android → Build App Bundle (Google Play).
./tools/build.sh android    # rebuild → produces SnakeLadderPrototype.aab
```

Then in https://play.google.com/console:

1. **Create app** → fill in title / short description / full description / screenshots / icon / category.
2. **Release → Production → Create release** → upload the `.aab`.
3. Fill in **Content rating**, **Target audience**, **Data safety** questionnaires.
4. **Review and roll out**.

Typical Google review time: a few hours to a few days.

## What's already configured

| Setting | iOS | Android |
|---|---|---|
| Bundle id | `com.yourstudio.snakesandladders` | `com.yourstudio.snakesandladders` |
| Display name | Snakes and Ladders | Snakes and Ladders |
| Version | 1.0.0 (build 1) | 1.0.0 (code 1) |
| Min OS | iOS 13.0 | Android 7.0 (API 24) |
| Target arch | ARM64 only | ARM64 only |
| Scripting | IL2CPP | IL2CPP |
| Orientation | Landscape locked | Landscape locked |
| Fullscreen | yes | yes |
| Status bar | hidden | n/a |
| Shadows | disabled | disabled |
| Frame rate cap | 30 fps | 30 fps |

Change the `BundleId` constant at the top of `Assets/Editor/DeploymentBuild.cs` to your own reverse-DNS prefix before building for production.
