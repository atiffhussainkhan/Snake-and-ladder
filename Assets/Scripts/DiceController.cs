using System.Collections.Generic;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class DiceController
    {
        public static GameObject Dice { get; private set; }
        public static int CurrentPlayer { get; private set; } = 1;
        public static bool GameEnded { get; private set; }

        // Standard die layout — opposite faces sum to 7.
        private static readonly (int face, int number, Vector3 normal, Vector3 up)[] FaceTable =
        {
            (0, 1, Vector3.up,      Vector3.back),
            (1, 6, Vector3.down,    Vector3.forward),
            (2, 2, Vector3.forward, Vector3.up),
            (3, 5, Vector3.back,    Vector3.up),
            (4, 3, Vector3.right,   Vector3.up),
            (5, 4, Vector3.left,    Vector3.up),
        };

        // Same colour on every face — the dice reads as one solid object and only the rolled
        // face's number pops out in contrast. HighlightSelectedNumber is what creates the
        // per-face differentiation at roll time.
        private static readonly Color FaceColor = new Color(1.00f, 0.97f, 0.85f);   // ivory

        // Number→contrast colour used only when that number's face is selected. Distinct colours
        // so each result reads clearly when it pops.
        private static readonly Color[] SelectedNumberColors =
        {
            new Color(0.10f, 0.10f, 0.10f),   // 1 — dark on ivory
            new Color(0.20f, 0.30f, 0.95f),   // 6 — vivid blue
            new Color(0.95f, 0.15f, 0.10f),   // 2 — vivid red
            new Color(0.10f, 0.70f, 0.30f),   // 5 — vivid green
            new Color(0.95f, 0.70f, 0.10f),   // 3 — vivid yellow-orange
            new Color(0.55f, 0.20f, 0.95f),   // 4 — vivid purple
        };

        // Per-face runtime references — populated in CreateFacePanel, mutated by HighlightSelectedNumber.
        private static readonly Dictionary<int, TextMesh> _numberMeshes = new Dictionary<int, TextMesh>();
        private static readonly Dictionary<int, Material> _plateMats = new Dictionary<int, Material>();
        private static readonly Dictionary<int, Material> _chipMats  = new Dictionary<int, Material>();
        private static int _lastSelectedNumber = -1;

        private static Material _diceMat;
        private static PawnMover _pawnMover;

        public static void SpawnDice()
        {
            Dice = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Dice.name = "Dice";
            Object.Destroy(Dice.GetComponent<SphereCollider>());

            Dice.transform.position = new Vector3(11.5f, 1.05f, 4.5f);
            Dice.transform.rotation = Quaternion.Euler(22, 40, 12);
            Dice.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            Dice.GetComponent<Renderer>().sharedMaterial = AcquireDiceMat();

            foreach (var f in FaceTable) CreateFacePanel(f.number, f.normal, f.up);

            // Initially every face is "embossed" (no number pops). Resetting each turn does the same.
            ResetNumberHighlights();

            Dice.AddComponent<DiceRoller>();
            Dice.AddComponent<DiceKeyboardHandler>();
        }

        public static void Cleanup()
        {
            if (_diceMat != null) { Object.Destroy(_diceMat); _diceMat = null; }
            Dice = null;
            CurrentPlayer = 1;
            GameEnded = false;
            _pawnMover = null;
            _numberMeshes.Clear();
            _plateMats.Clear();
            _chipMats.Clear();
            _lastSelectedNumber = -1;
        }

        public static void ResetForNewGame()
        {
            CurrentPlayer = 1;
            GameEnded = false;
            ResetNumberHighlights();
        }

        public static void SetPawnMover(PawnMover mover)
        {
            _pawnMover = mover;
        }

        // Called by DiceRoller after the dice settles — pushes the chosen face's number
        // into the bright "selected" state and resets all the others.
        public static void HighlightSelectedNumber(int number)
        {
            if (_lastSelectedNumber == number) return;
            ResetNumberHighlights();
            if (!_numberMeshes.TryGetValue(number, out var numMesh)) return;

            numMesh.color = SelectedNumberColors[number - 1];
            numMesh.fontSize = 140;                     // bigger when selected
            numMesh.characterSize = 0.20f;
            numMesh.fontStyle = FontStyle.Bold;

            if (_plateMats.TryGetValue(number, out var plateMat))
                plateMat.SetColor("_EmissionColor", FaceColor * 1.6f);
            if (_chipMats.TryGetValue(number, out var chipMat))
            {
                Color tinted = Color.Lerp(FaceColor, SelectedNumberColors[number - 1], 0.55f);
                chipMat.color = Color.Lerp(FaceColor, Color.white, 0.50f);
                chipMat.SetColor("_EmissionColor", tinted * 0.9f);
            }
            _lastSelectedNumber = number;
        }

        // Restores every face to the default "embossed" look: number rendered in the same
        // colour as the face background (low contrast, looks stamped into the surface),
        // gentle panel emission, neutral chip.
        public static void ResetNumberHighlights()
        {
            for (int i = 1; i <= 6; i++)
            {
                if (_numberMeshes.TryGetValue(i, out var numMesh))
                {
                    numMesh.color = FaceColor;          // blends into face — embossed
                    numMesh.fontSize = 90;
                    numMesh.characterSize = 0.14f;
                }
                if (_plateMats.TryGetValue(i, out var plateMat))
                    plateMat.SetColor("_EmissionColor", FaceColor * 0.20f);
                if (_chipMats.TryGetValue(i, out var chipMat))
                {
                    chipMat.color = Color.Lerp(FaceColor, Color.white, 0.30f);
                    chipMat.SetColor("_EmissionColor", Color.Lerp(FaceColor, Color.white, 0.30f) * 0.20f);
                }
            }
            _lastSelectedNumber = -1;
        }

        private static Material AcquireDiceMat()
        {
            if (_diceMat != null) return _diceMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.94f, 0.88f, 0.72f);
            mat.SetFloat("_Glossiness", 0.55f);
            mat.SetFloat("_Metallic", 0.05f);
            mat.mainTexture = WoodTextureGen.GetWoodA();
            mat.SetTextureScale("_MainTex", new Vector2(2f, 2f));
            mat.SetTexture("_BumpMap", WoodTextureGen.WoodNormal(256, 0.5f));
            mat.SetFloat("_BumpScale", 0.25f);
            _diceMat = mat;
            return mat;
        }

        // Each face is a concentric stack: ivory plate → slightly lighter chip on top → TextMesh.
        // The TextMesh defaults to the plate colour so the number blends in (engraved look) —
        // HighlightSelectedNumber flips the chosen face's number to a vivid contrasting colour.
        private static void CreateFacePanel(int number, Vector3 normal, Vector3 up)
        {
            var panel = new GameObject($"FacePanel_{number}");
            panel.transform.SetParent(Dice.transform, false);
            panel.transform.localPosition = normal * 0.81f;
            panel.transform.localRotation = Quaternion.LookRotation(normal, up);

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = $"FacePlate_{number}";
            Object.Destroy(plate.GetComponent<BoxCollider>());
            plate.transform.SetParent(panel.transform, false);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localScale = new Vector3(0.78f, 0.78f, 0.05f);
            var plateMat = MakeFaceMat();
            plate.GetComponent<Renderer>().sharedMaterial = plateMat;
            _plateMats[number] = plateMat;

            var chip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chip.name = $"FaceChip_{number}";
            Object.Destroy(chip.GetComponent<BoxCollider>());
            chip.transform.SetParent(plate.transform, false);
            chip.transform.localPosition = new Vector3(0, 0, 0.55f);
            chip.transform.localScale = new Vector3(0.72f, 0.72f, 0.18f);
            var chipMat = MakeChipMat();
            chip.GetComponent<Renderer>().sharedMaterial = chipMat;
            _chipMats[number] = chipMat;

            var numGo = new GameObject($"FaceNum_{number}");
            numGo.transform.SetParent(chip.transform, false);
            numGo.transform.localPosition = new Vector3(0, 0, 0.6f);

            var text = numGo.AddComponent<TextMesh>();
            text.text = number.ToString();
            text.fontSize = 90;
            text.characterSize = 0.14f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = FaceColor; // embossed by default — same colour as the face
            text.fontStyle = FontStyle.Bold;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _numberMeshes[number] = text;
        }

        private static Material MakeFaceMat()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = FaceColor;
            mat.SetFloat("_Glossiness", 0.55f);
            mat.SetFloat("_Metallic", 0.10f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", FaceColor * 0.20f);
            return mat;
        }

        private static Material MakeChipMat()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(FaceColor, Color.white, 0.30f);
            mat.SetFloat("_Glossiness", 0.75f);
            mat.SetFloat("_Metallic", 0.05f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.Lerp(FaceColor, Color.white, 0.30f) * 0.20f);
            return mat;
        }

        // Player input entry point. The full turn sequence (dice roll → pawn steps → snake/ladder jump → next turn)
        // is driven from inside DiceController via DiceRoller and PawnMover.
        public static void Roll()
        {
            if (GameEnded) return;
            if (_pawnMover == null) return;

            int roll = UnityEngine.Random.Range(1, 7);
            int currentPlayer = CurrentPlayer;
            int currentSquare = currentPlayer == 1 ? PawnSpawner.Pawn1Square : PawnSpawner.Pawn2Square;
            int target = Mathf.Min(100, currentSquare + roll);

            Dice.GetComponent<DiceRoller>().StartRoll(roll, (finalRoll) => OnDiceLanded(currentPlayer, currentSquare, target, finalRoll));
        }

        private static void OnDiceLanded(int player, int fromSquare, int targetSquare, int finalRoll)
        {
            // The dice just settled — push the chosen face's number into the prominent state.
            HighlightSelectedNumber(finalRoll);

            var runner = _pawnMover;
            if (runner == null) return;
            runner.StartCoroutine(RunTurnAnimation(player, fromSquare, targetSquare, finalRoll));
        }

        private static System.Collections.IEnumerator RunTurnAnimation(int player, int fromSquare, int targetSquare, int finalRoll)
        {
            yield return _pawnMover.StartCoroutine(_pawnMover.MoveStepByStep(player, fromSquare, targetSquare));

            int resolved = BoardBuilder.ResolveSquare(targetSquare);
            if (resolved != targetSquare)
            {
                yield return _pawnMover.StartCoroutine(_pawnMover.JumpTo(player, resolved));
            }

            if (resolved == 100 || targetSquare == 100)
            {
                GameEnded = true;
                UIHud.ShowWin(player);
                if (Dice != null)
                {
                    var st = Dice.GetComponent<StressTest>();
                    if (st != null) st.enabled = false;
                }
                yield break;
            }

            // Turn is over — un-highlight the dice so the next roll starts from the embossed state.
            CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;
            UIHud.UpdateTurn(CurrentPlayer);
            ResetNumberHighlights();
        }
    }

    // Space / R rolls the dice without needing to click the Game view.
    public class DiceKeyboardHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
            {
                DiceController.Roll();
            }
        }
    }
}
