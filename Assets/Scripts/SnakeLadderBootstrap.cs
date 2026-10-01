using UnityEngine;

namespace SnakeLadder.Prototype
{
    public class SnakeLadderBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (FindAnyObjectByType<SnakeLadderBootstrap>() != null) return;

            // Mobile-friendly defaults BEFORE anything else gets built. Capping FPS cuts
            // battery drain on old devices and avoids GPU/CPU spinning past what's visible.
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.antiAliasing = 0;
            QualitySettings.particleRaycastBudget = 0;
            QualitySettings.skinWeights = SkinWeights.OneBone;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            var go = new GameObject("SnakeLadderBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<SnakeLadderBootstrap>();
            go.AddComponent<PawnMover>();
            go.AddComponent<QuitInputHandler>();
        }

        private bool _quitting;

        private void Start()
        {
            EnvironmentBuilder.BuildEnvironment();
            BoardBuilder.BuildBoard();
            PawnSpawner.SpawnPawns();
            SnakeSpawner.SpawnSnakes();
            LadderSpawner.SpawnLadders();
            DiceController.SpawnDice();
            CameraSetup.ConfigureCamera();
            UIHud.SpawnUI();

            // Hand the shared PawnMover to DiceController so it can drive the
            // step-by-step + snake/ladder animation coroutines.
            DiceController.SetPawnMover(GetComponent<PawnMover>());

            DiceController.Dice.AddComponent<StressTest>();
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
            ReleaseAll();
        }

        private void OnDestroy()
        {
            if (_quitting) return; // OnApplicationQuit already ran the cleanup.
            ReleaseAll();
        }

        // Tear down every cached resource in reverse build order, then drop the references
        // the static state held so the GC can reclaim them.
        private void ReleaseAll()
        {
            // Stop coroutines first: a dice-roll animation started on frame N must not still
            // be ticking when OnApplicationQuit fires on frame N+5 — that would leave the
            // GameObject / cached assets in an inconsistent state across the teardown below.
            StopAllCoroutines();

            UIHud.Cleanup();
            DiceController.Cleanup();
            LadderSpawner.Cleanup();
            SnakeSpawner.Cleanup();
            PawnSpawner.Cleanup();
            BoardBuilder.Cleanup();
            EnvironmentBuilder.Cleanup();
            WoodTextureGen.Cleanup();

            // Sweep any orphaned assets we may have missed (async — completes on a later frame).
            Resources.UnloadUnusedAssets();

            // At shutdown there's no risk of mid-game hitches, so force a managed-heap sweep
            // so the OS reclaims the freed pages immediately rather than waiting for the next
            // allocation pressure.
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }
    }

    // Esc keys to quit so the same teardown path fires whether the player hits Esc,
    // closes the window, or the OS kills the process.
    public class QuitInputHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
