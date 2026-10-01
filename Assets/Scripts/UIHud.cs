using UnityEngine;
using UnityEngine.UI;

namespace SnakeLadder.Prototype
{
    public static class UIHud
    {
        private static GameObject turnTextGo;
        private static GameObject winBannerGo;
        private static Text turnText;
        private static Font _legacyFont;

        private static Font LegacyFont => _legacyFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static void SpawnUI()
        {
            var hudGo = new GameObject("HUD");

            var canvas = hudGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = hudGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            hudGo.AddComponent<GraphicRaycaster>();

            turnTextGo = CreateText("TurnText",
                "Player 1 (RED) — press SPACE or R to roll",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -90), 28, Color.white);
            turnTextGo.transform.SetParent(hudGo.transform);
            turnText = turnTextGo.GetComponent<Text>();

            CreateText("Disclaimer",
                "PROTOTYPE - NON-PRODUCTION\nRules are mock visuals. See docs/PHASE_3_PROTOTYPE_AUTHORIZATION.md",
                new Vector2(0, 0), new Vector2(0.6f, 0), new Vector2(0, 0), 16, new Color(1f, 0.85f, 0.30f))
                .transform.SetParent(hudGo.transform);

            winBannerGo = CreateText("WinBanner", "",
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), 64, Color.yellow);
            winBannerGo.transform.SetParent(hudGo.transform);
            winBannerGo.SetActive(false);
        }

        public static void Cleanup()
        {
            turnTextGo = null;
            winBannerGo = null;
            turnText = null;
        }

        private static GameObject CreateText(string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, int fontSize, Color color)
        {
            var go = new GameObject(name);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            if (anchorMin == anchorMax)
            {
                rect.pivot = anchorMin;
                rect.anchoredPosition = anchoredPos;
                rect.sizeDelta = new Vector2(600, 80);
            }
            else
            {
                rect.offsetMin = new Vector2(20, anchoredPos.y + (anchorMin.y == 0 ? 10 : 0));
                rect.offsetMax = new Vector2(-20, anchoredPos.y + (anchorMin.y == 0 ? 0 : -10));
            }
            go.AddComponent<CanvasRenderer>();
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = LegacyFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            return go;
        }

        public static void UpdateTurn(int player)
        {
            if (turnText == null) return;
            string name = player == 1 ? "1 (RED)" : "2 (BLUE)";
            turnText.text = $"Player {name}'s turn  -  press SPACE or R to roll";
        }

        public static void ShowWin(int winningPlayer)
        {
            if (winBannerGo == null) return;
            winBannerGo.SetActive(true);
            var text = winBannerGo.GetComponent<Text>();
            text.text = $"PLAYER {winningPlayer} WINS!";
            text.alignment = TextAnchor.MiddleCenter;
        }

        public static void ResetForNewGame()
        {
            if (winBannerGo != null) winBannerGo.SetActive(false);
            UpdateTurn(1);
        }
    }
}
