using System.IO;
using UnityEditor;
using UnityEngine;

namespace SnakeLadder.Prototype.EditorTools
{
    public static class StressTestRunner
    {
        private const int GamesToRun = 1000;
        private const int MaxTurnsPerGame = 500;
        private static readonly string SentinelFile = Path.Combine(Application.dataPath, "..", "Library", "stress_test.trigger");

        [MenuItem("Tools/Snake-Ladder/Run Stress Test (1000 games)")]
        public static void RunFromMenu() => Run();

        [InitializeOnLoadMethod]
        private static void ArmWatcher()
        {
            EditorApplication.update += Tick;
        }

        private static double _nextCheckTime;
        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup < _nextCheckTime) return;
            _nextCheckTime = EditorApplication.timeSinceStartup + 1.0;
            if (!File.Exists(SentinelFile)) return;
            File.Delete(SentinelFile);
            Debug.Log($"[StressTestRunner] Sentinel detected, running {GamesToRun}-game stress test");
            Run();
        }

        public static void Run()
        {
            var stats = StressSimulator.RunBatch(GamesToRun, MaxTurnsPerGame);
            StressTest.LogStats(stats, GamesToRun);

            if (stats.errors == 0)
                Debug.Log("[StressTestRunner] PASS — no exceptions during simulation");
            else
                Debug.LogError($"[StressTestRunner] FAIL — {stats.errors} game(s) threw exceptions");
        }
    }
}
