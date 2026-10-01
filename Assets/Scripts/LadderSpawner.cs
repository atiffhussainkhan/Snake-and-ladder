using System.Collections.Generic;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class LadderSpawner
    {
        // Paired 1:1 with BoardBuilder.Ladders (4→14, 9→31, 21→39, 28→56, 36→57, 51→67, 62→81, 71→91).
        // Brightened vs the previous palette so each ladder pops on the wooden board.
        private static readonly Color[] LadderColors =
        {
            new Color(1.00f, 0.20f, 0.25f),   // crimson red
            new Color(1.00f, 0.55f, 0.05f),   // vivid orange
            new Color(1.00f, 0.90f, 0.10f),   // bright gold
            new Color(0.25f, 0.90f, 0.45f),   // emerald green
            new Color(0.15f, 0.55f, 1.00f),   // royal blue
            new Color(0.70f, 0.25f, 1.00f),   // vivid purple
            new Color(1.00f, 0.35f, 0.75f),   // hot pink
            new Color(0.15f, 0.95f, 0.90f),   // cyan
        };

        private static readonly Dictionary<Color, Material> _railMats   = new Dictionary<Color, Material>();
        private static readonly Dictionary<Color, Material> _bubbleMats = new Dictionary<Color, Material>();
        private static readonly Dictionary<Color, Material> _finialMats = new Dictionary<Color, Material>();

        public static void SpawnLadders()
        {
            // Dictionary<int,int> enumeration order is not guaranteed stable; copy into a list
            // and sort by key so ladder colours are paired deterministically with ladder pairs.
            var ordered = new List<KeyValuePair<int, int>>(BoardBuilder.Ladders);
            ordered.Sort((a, b) => a.Key.CompareTo(b.Key));
            int count = ordered.Count;
            for (int i = 0; i < count; i++)
            {
                // Clamp instead of modulo: if more ladders exist than colours, the extras reuse
                // the last colour instead of silently wrapping back to the first.
                int colorIdx = i < LadderColors.Length ? i : LadderColors.Length - 1;
                var kv = ordered[i];
                CreateLadder(kv.Key, kv.Value, LadderColors[colorIdx]);
            }
        }

        // Destroys every cached ladder material. Safe to call multiple times.
        public static void Cleanup()
        {
            foreach (var m in _railMats.Values)   if (m != null) Object.Destroy(m);
            foreach (var m in _bubbleMats.Values) if (m != null) Object.Destroy(m);
            foreach (var m in _finialMats.Values) if (m != null) Object.Destroy(m);
            _railMats.Clear();
            _bubbleMats.Clear();
            _finialMats.Clear();
        }

        private static void CreateLadder(int fromSquare, int toSquare, Color ladderColor)
        {
            var start = BoardBuilder.PositionForSquare(fromSquare);
            var end   = BoardBuilder.PositionForSquare(toSquare);
            start.y = 0.12f;
            end.y   = 0.12f;

            var parent = new GameObject($"Ladder_{fromSquare}_to_{toSquare}");
            parent.transform.position = Vector3.zero;

            var railMat   = AcquireRailMat(ladderColor);
            var bubbleMat = AcquireBubbleMat(ladderColor);
            var finialMat = AcquireFinialMat(ladderColor);

            // Two thin, wavy, snake-shaped rails flanking the ladder. Wider spacing (0.22) so the
            // bubbles between them never clip into the wavy rail edges.
            CreateWavyRail(parent.transform, start, end, new Vector3(-0.22f, 0, 0), railMat);
            CreateWavyRail(parent.transform, start, end, new Vector3( 0.22f, 0, 0), railMat);

            // Bubble finials at the top of each rail — soft orb terminations.
            CreateFinial(parent.transform, end + new Vector3(-0.22f, 0, 0) + Vector3.up * 0.10f, finialMat);
            CreateFinial(parent.transform, end + new Vector3( 0.22f, 0, 0) + Vector3.up * 0.10f, finialMat);

            // Bubble rungs — spheres connecting the two rails at evenly spaced points.
            float dist = Vector3.Distance(start, end);
            int rungCount = Mathf.Clamp(Mathf.RoundToInt(dist * 1.4f), 5, 9);
            for (int r = 1; r <= rungCount; r++)
            {
                float tt = r / (float)(rungCount + 1);
                Vector3 rungPos = Vector3.Lerp(start, end, tt);
                CreateBubble(parent.transform, rungPos, bubbleMat);
            }
        }

        // Wavy snake-shaped rail: a thin tube with a subtle perpendicular wave along its length.
        // Reads as a soft curving cord rather than a stiff pole.
        private static void CreateWavyRail(Transform parent, Vector3 start, Vector3 end, Vector3 offset, Material railMat)
        {
            const int samples = 32;
            Vector3[] points = new Vector3[samples];
            float[] radii   = new float[samples];

            Vector3 axis = end - start;
            Vector3 waveDir = Vector3.Cross(Vector3.up, axis).normalized;
            if (waveDir.sqrMagnitude < 0.001f) waveDir = Vector3.right;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)(samples - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t) + offset;
                // Subtle sinusoidal offset (amplitude 0.04) so the rail feels organic without obscuring the board.
                float wave = Mathf.Sin(t * Mathf.PI * 3.2f) * 0.025f;
                // Tighter wave amplitude so the wavy rail never closes in on the bubble rungs.
                points[i] = basePos + waveDir * wave;
                // Slightly thicker at the bottom, tapered at the top — chunky enough to read
                // against the wooden board without obscuring neighbouring squares.
                radii[i] = Mathf.Lerp(0.080f, 0.060f, t);
            }

            MeshUtils.CreateTube(parent, "Rail", points, radii, railMat, radialSegments: 12);
        }

        private static void CreateBubble(Transform parent, Vector3 pos, Material bubbleMat)
        {
            var bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bubble.name = "Bubble";
            bubble.transform.SetParent(parent);
            bubble.transform.position = pos;
            bubble.transform.localScale = new Vector3(0.17f, 0.17f, 0.17f);
            Object.Destroy(bubble.GetComponent<SphereCollider>());
            bubble.GetComponent<Renderer>().sharedMaterial = bubbleMat;
        }

        private static void CreateFinial(Transform parent, Vector3 pos, Material finialMat)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Finial";
            ball.transform.SetParent(parent);
            ball.transform.position = pos;
            ball.transform.localScale = new Vector3(0.17f, 0.17f, 0.17f);
            Object.Destroy(ball.GetComponent<SphereCollider>());
            ball.GetComponent<Renderer>().sharedMaterial = finialMat;
        }

        private static Material AcquireRailMat(Color ladderColor)
        {
            Color key = RoundColor(ladderColor);
            if (_railMats.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = ladderColor;
            mat.SetFloat("_Glossiness", 0.65f);
            mat.SetFloat("_Metallic", 0.15f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", ladderColor * 0.25f);
            _railMats[key] = mat;
            return mat;
        }

        private static Material AcquireBubbleMat(Color ladderColor)
        {
            Color key = RoundColor(ladderColor);
            if (_bubbleMats.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(ladderColor, Color.white, 0.20f);
            mat.SetFloat("_Glossiness", 0.80f);
            mat.SetFloat("_Metallic", 0.10f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", ladderColor * 0.30f);
            _bubbleMats[key] = mat;
            return mat;
        }

        private static Material AcquireFinialMat(Color ladderColor)
        {
            Color key = RoundColor(ladderColor);
            if (_finialMats.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = ladderColor;
            mat.SetFloat("_Glossiness", 0.70f);
            mat.SetFloat("_Metallic", 0.20f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", ladderColor * 0.35f);
            _finialMats[key] = mat;
            return mat;
        }

        private static Color RoundColor(Color c)
        {
            return new Color(Mathf.Round(c.r * 4f) / 4f, Mathf.Round(c.g * 4f) / 4f, Mathf.Round(c.b * 4f) / 4f);
        }
    }
}
