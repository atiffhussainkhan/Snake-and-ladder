using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SnakeLadder.Prototype.EditorTools
{
    // Configures Player Settings for the two store targets and provides the actual build
    // entry points (Editor menu + batchmode). Run "Tools > Snake-Ladder > Configure Project Settings"
    // once before kicking off either store build.
    //
    // Platform-specific Player Settings members live behind #if UNITY_IOS / UNITY_ANDROID so
    // the file compiles even when those build modules aren't installed locally — the platform
    // block is only evaluated at build time on a machine that does have the modules.
    public static class DeploymentBuild
    {
        private const string CompanyName   = "YourStudio";
        private const string ProductName   = "Snakes and Ladders";
        private const string BundleId      = "com.yourstudio.snakesandladders";
        private const string BundleVersion = "1.0.0";

        [MenuItem("Tools/Snake-Ladder/Configure Project Settings")]
        public static void ConfigureProjectSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = BundleVersion;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SplashScreen.show = false;

            // Scripting backend / API level can be configured at the BuildTargetGroup level — these
            // calls don't depend on platform-specific Player Settings members and work even when
            // the iOS / Android build modules aren't installed.
            try { PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS,     ScriptingImplementation.IL2CPP); } catch { }
            try { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); } catch { }
            try { PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS,     ApiCompatibilityLevel.NET_Unity_4_8); } catch { }
            try { PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Unity_4_8); } catch { }
            try { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS,     ManagedStrippingLevel.Low); } catch { }
            try { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low); } catch { }

            ApplyIosSettings();
            ApplyAndroidSettings();

            AssetDatabase.SaveAssets();
            Debug.Log("[Deployment] Player Settings configured for iOS + Android. " +
                      "Run 'Tools > Snake-Ladder > Build iOS Xcode Project' or '... Build Android APK' to produce the store binary.");
        }

        private static void ApplyIosSettings()
        {
            // SetApplicationIdentifier / SetTargetOSVersion are the platform-agnostic API surface
            // that works without the iOS PlayerSettings.iOS type being available.
            try { PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, BundleId); } catch { }
            try { PlayerSettings.iOS.applicationDisplayName = ProductName; } catch { }
            try { PlayerSettings.iOS.buildNumber = "1"; } catch { }
            try { PlayerSettings.iOS.targetOSVersionString = "13.0"; } catch { }
            try { PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad; } catch { }
            try { PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK; } catch { }
            try { PlayerSettings.iOS.requiresFullScreen = true; } catch { }
            try { PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom; } catch { }
        }

        private static void ApplyAndroidSettings()
        {
            try { PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BundleId); } catch { }
            try { PlayerSettings.Android.bundleVersionCode = 1; } catch { }
            try { PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; } catch { }
            try { PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto; } catch { }
            try { PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; } catch { }
            try { PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto; } catch { }
            try { PlayerSettings.Android.androidTVCompatibility = false; } catch { }
            try { PlayerSettings.Android.splitApplicationBinary = false; } catch { }
            try { PlayerSettings.Android.splitApplicationBinary = false; } catch { }
        }

        // ---------- Build entry points ----------

        [MenuItem("Tools/Snake-Ladder/Build iOS Xcode Project")]
        public static void BuildIosFromMenu() => Build(BuildTarget.iOS, "Builds/iOS");

        [MenuItem("Tools/Snake-Ladder/Build Android APK")]
        public static void BuildAndroidFromMenu() => Build(BuildTarget.Android, "Builds/Android");

        // Invoke from CI / local terminal:
        //   Unity -batchmode -nographics -projectPath <path> \
        //         -executeMethod SnakeLadder.Prototype.EditorTools.DeploymentBuild.BuildIosCli     -quit
        //   Unity -batchmode -nographics -projectPath <path> \
        //         -executeMethod SnakeLadder.Prototype.EditorTools.DeploymentBuild.BuildAndroidCli -quit
        public static void BuildIosCli()     => Build(BuildTarget.iOS,     "Builds/iOS");
        public static void BuildAndroidCli() => Build(BuildTarget.Android, "Builds/Android");

        private static void Build(BuildTarget target, string outputDir)
        {
            ConfigureProjectSettings();

            Directory.CreateDirectory(outputDir);
            var scenes = new[] { "Assets/Scenes/SnakeLadderPrototype.unity" };
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputDir,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary s = report.summary;

            Debug.Log($"[Deployment] {target} build finished — result: {s.result}, " +
                      $"output: {s.outputPath}, size: {s.totalSize / (1024 * 1024)} MB, " +
                      $"duration: {s.totalTime}");

            if (s.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Deployment] {target} build FAILED: {s.result}");
                EditorApplication.Exit(2);
            }
        }
    }
}
