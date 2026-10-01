using System.Collections.Generic;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class SnakeSpawner
    {
        // Paired 1:1 with BoardBuilder.Snakes (99→54, 70→55, 52→42, 25→2, 95→72, 64→18).
        // Vivid saturated colours — the snakes need to pop hard off the wooden board.
        private static readonly Color[] SnakeColors =
        {
            new Color(0.05f, 0.45f, 1.00f),   // cobalt blue
            new Color(1.00f, 0.15f, 0.55f),   // candy pink
            new Color(0.65f, 0.05f, 1.00f),   // deep purple
            new Color(1.00f, 0.80f, 0.05f),   // warm gold
            new Color(1.00f, 1.00f, 0.90f),   // bright ash-silver
            new Color(0.95f, 0.45f, 0.10f),   // earthy orange-brown
        };

        private static readonly float[] Phase = { 0.0f, 1.4f, 0.6f, 2.1f, 1.0f, 2.6f };

        // Material caches — keyed by rounded colour to share across snakes of similar hue.
        private static readonly Dictionary<Color, Material> _bodyMats  = new Dictionary<Color, Material>();
        private static readonly Dictionary<Color, Material> _ridgeMats = new Dictionary<Color, Material>();
        private static readonly Dictionary<Color, Material> _headMats  = new Dictionary<Color, Material>();
        private static Material _irisMat;
        private static Material _pupilMat;
        private static Material _tongueMat;
        private static Material _prongMat;

        public static void SpawnSnakes()
        {
            // Dictionary enumeration order is not guaranteed stable across runtimes, so materialise
            // a sorted snapshot (ascending by head square) and iterate it deterministically.
            var ordered = new List<KeyValuePair<int, int>>(BoardBuilder.Snakes);
            ordered.Sort((a, b) => a.Key.CompareTo(b.Key));
            int lastColorIdx = SnakeColors.Length - 1;
            int lastPhaseIdx = Phase.Length - 1;
            for (int i = 0; i < ordered.Count; i++)
            {
                var kv = ordered[i];
                int colorIdx = i <= lastColorIdx ? i : lastColorIdx;
                int phaseIdx = i <= lastPhaseIdx ? i : lastPhaseIdx;
                CreateSnake(kv.Key, kv.Value, SnakeColors[colorIdx], Phase[phaseIdx]);
            }
        }

        // Destroys every cached snake material. Safe to call multiple times.
        public static void Cleanup()
        {
            foreach (var m in _bodyMats.Values)  if (m != null) Object.Destroy(m);
            foreach (var m in _ridgeMats.Values) if (m != null) Object.Destroy(m);
            foreach (var m in _headMats.Values)  if (m != null) Object.Destroy(m);
            if (_irisMat   != null) { Object.Destroy(_irisMat);   _irisMat = null; }
            if (_pupilMat  != null) { Object.Destroy(_pupilMat);  _pupilMat = null; }
            if (_tongueMat != null) { Object.Destroy(_tongueMat); _tongueMat = null; }
            if (_prongMat  != null) { Object.Destroy(_prongMat);  _prongMat = null; }
            _bodyMats.Clear();
            _ridgeMats.Clear();
            _headMats.Clear();
        }

        private static void CreateSnake(int fromSquare, int toSquare, Color bodyColor, float phase)
        {
            // High above the tile surface (tile top is y=0.18) — the snake needs to clearly read
            // as a creature hovering above the board, not embedded in the wood.
            const float baseY = 0.55f;
            var start = BoardBuilder.PositionForSquare(fromSquare); start.y = baseY;
            var end   = BoardBuilder.PositionForSquare(toSquare);   end.y   = baseY;

            var parent = new GameObject($"Snake_{fromSquare}_to_{toSquare}");
            parent.transform.position = Vector3.zero;

            const int samples = 60;
            Vector3[] points = new Vector3[samples];
            float[] radii   = new float[samples];

            // Serpentine path with multiple S-curves; belly peak sits ~35% down the body.
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)(samples - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.x += Mathf.Sin(t * Mathf.PI * 2.2f + phase) * 0.78f;
                pos.x += Mathf.Sin(t * Mathf.PI * 0.95f + phase * 0.5f) * 0.34f;
                pos.y += Mathf.Sin(t * Mathf.PI * 1.6f + phase * 0.3f) * 0.08f;
                points[i] = pos;

                float bulge = Mathf.Sin(Mathf.Clamp01(t * 1.4f) * Mathf.PI);
                float tailTaper = Mathf.Pow(1f - t, 0.75f);
                float headTaper = Mathf.SmoothStep(0f, 0.10f, t);
                // Very chunky body — the snake should clearly read as a solid creature against the wood.
                radii[i] = Mathf.Lerp(0.28f, 0.55f, bulge) * (0.60f + 0.40f * tailTaper) * headTaper;
            }

            MeshUtils.CreateTube(parent.transform, "Body", points, radii, AcquireBodyMat(bodyColor), radialSegments: 18);
            CreateScaleRidges(parent.transform, points, radii, bodyColor);
            CreateSnakeHead(parent.transform, points, bodyColor);
        }

        private static Material AcquireBodyMat(Color bodyColor)
        {
            Color key = RoundColor(bodyColor);
            if (_bodyMats.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = MakeSnakeMaterial(bodyColor);
            mat.SetTextureScale("_MainTex", new Vector2(3f, 8f));
            mat.SetTexture("_BumpMap", WoodTextureGen.ScaleNormal(256, 2.5f));
            mat.SetFloat("_BumpScale", 1.6f);
            _bodyMats[key] = mat;
            return mat;
        }

        private static Material AcquireRidgeMat(Color bodyColor)
        {
            Color key = RoundColor(bodyColor);
            if (_ridgeMats.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = Color.Lerp(bodyColor, bodyColor * 0.55f, 0.55f);
            mat.SetFloat("_Glossiness", 0.90f);
            mat.SetFloat("_Metallic", 0.45f);
            mat.mainTexture = WoodTextureGen.GetScale(bodyColor);
            mat.SetTextureScale("_MainTex", new Vector2(2f, 1f));
            mat.SetTexture("_BumpMap", WoodTextureGen.ScaleNormal(256, 1.0f));
            mat.SetFloat("_BumpScale", 0.5f);
            mat.SetFloat("_Mode", 0);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.Lerp(bodyColor, bodyColor * 0.55f, 0.55f) * 0.9f);
            _ridgeMats[key] = mat;
            return mat;
        }

        private static Material AcquireHeadMat(Color bodyColor)
        {
            Color key = RoundColor(bodyColor);
            if (_headMats.TryGetValue(key, out var cached) && cached != null) return cached;
            var mat = MakeSnakeMaterial(bodyColor * 1.10f);
            mat.SetTextureScale("_MainTex", new Vector2(4f, 3f));
            mat.SetColor("_EmissionColor", bodyColor * 0.95f);
            _headMats[key] = mat;
            return mat;
        }

        private static Material AcquireIrisMat()
        {
            if (_irisMat != null) return _irisMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1.0f, 0.82f, 0.20f);
            mat.SetFloat("_Glossiness", 0.95f);
            mat.SetFloat("_Metallic", 0.10f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.35f, 0.25f, 0.05f));
            _irisMat = mat;
            return mat;
        }

        private static Material AcquirePupilMat()
        {
            if (_pupilMat != null) return _pupilMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.02f, 0.02f, 0.02f);
            mat.SetFloat("_Glossiness", 0.95f);
            _pupilMat = mat;
            return mat;
        }

        private static Material AcquireTongueMat()
        {
            if (_tongueMat != null) return _tongueMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.92f, 0.10f, 0.22f);
            mat.SetFloat("_Glossiness", 0.60f);
            _tongueMat = mat;
            return mat;
        }

        private static Material AcquireProngMat()
        {
            if (_prongMat != null) return _prongMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.85f, 0.08f, 0.18f);
            mat.SetFloat("_Glossiness", 0.60f);
            _prongMat = mat;
            return mat;
        }

        private static Color RoundColor(Color c)
        {
            return new Color(Mathf.Round(c.r * 4f) / 4f, Mathf.Round(c.g * 4f) / 4f, Mathf.Round(c.b * 4f) / 4f);
        }

        private static Material MakeSnakeMaterial(Color bodyColor)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = bodyColor;
            mat.SetFloat("_Glossiness", 0.35f);
            mat.SetFloat("_Metallic", 0.05f);
            mat.mainTexture = WoodTextureGen.GetScale(bodyColor);

            mat.SetFloat("_Mode", 0); // Opaque

            mat.EnableKeyword("_EMISSION");
            // Strong emission so the snake really pops off the wood.
            mat.SetColor("_EmissionColor", bodyColor * 0.95f);
            return mat;
        }

        private static void CreateScaleRidges(Transform parent, Vector3[] points, float[] radii, Color bodyColor)
        {
            var sharedMat = AcquireRidgeMat(bodyColor);
            for (int i = 6; i < points.Length - 6; i += 6)
            {
                if (radii[i] < 0.06f) continue;
                Vector3 tangent = (points[i + 1] - points[i - 1]).normalized;
                Vector3 up = Vector3.Cross(tangent, Vector3.forward);
                if (up.sqrMagnitude < 0.001f) up = Vector3.up;
                else up.Normalize();

                var ridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ridge.name = $"Dorsal{i}";
                ridge.transform.SetParent(parent);
                ridge.transform.position = points[i] + up * (radii[i] * 0.55f);
                ridge.transform.rotation = Quaternion.LookRotation(tangent, up);
                ridge.transform.localScale = new Vector3(radii[i] * 1.95f, 0.025f, radii[i] * 1.45f);
                Object.Destroy(ridge.GetComponent<BoxCollider>());
                ridge.GetComponent<Renderer>().sharedMaterial = sharedMat;
            }
        }

        private static void CreateSnakeHead(Transform parent, Vector3[] points, Color bodyColor)
        {
            const int headIndex = 3;
            Vector3 headPos = points[headIndex];
            Vector3 headForward = (points[headIndex + 3] - points[headIndex - 1]).normalized;

            var headMat = AcquireHeadMat(bodyColor);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(parent);
            head.transform.position = headPos - headForward * 0.04f;
            head.transform.localScale = new Vector3(0.36f, 0.30f, 0.52f);
            Object.Destroy(head.GetComponent<SphereCollider>());
            head.GetComponent<Renderer>().sharedMaterial = headMat;

            var snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snout.name = "Snout";
            snout.transform.SetParent(parent);
            snout.transform.position = headPos + headForward * 0.26f;
            snout.transform.localScale = new Vector3(0.22f, 0.18f, 0.28f);
            Object.Destroy(snout.GetComponent<SphereCollider>());
            snout.GetComponent<Renderer>().sharedMaterial = headMat;

            Vector3 right = Vector3.Cross(Vector3.up, headForward).normalized;
            CreateSnakeEye(parent, headPos + Vector3.up * 0.16f + headForward * 0.10f + right * 0.18f, headForward);
            CreateSnakeEye(parent, headPos + Vector3.up * 0.16f + headForward * 0.10f - right * 0.18f, headForward);
            CreateSnakeTongue(parent, headPos + headForward * 0.42f, headForward);
        }

        private static void CreateSnakeEye(Transform parent, Vector3 pos, Vector3 forward)
        {
            var iris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            iris.name = "Eye";
            iris.transform.SetParent(parent);
            iris.transform.position = pos + Vector3.up * 0.02f;
            iris.transform.localScale = new Vector3(0.13f, 0.13f, 0.10f);
            Object.Destroy(iris.GetComponent<SphereCollider>());
            iris.GetComponent<Renderer>().sharedMaterial = AcquireIrisMat();

            var pupil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pupil.name = "Pupil";
            pupil.transform.SetParent(parent);
            pupil.transform.position = pos + Vector3.up * 0.03f;
            pupil.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            pupil.transform.localScale = new Vector3(0.025f, 0.14f, 0.06f);
            Object.Destroy(pupil.GetComponent<BoxCollider>());
            pupil.GetComponent<Renderer>().sharedMaterial = AcquirePupilMat();
        }

        private static void CreateSnakeTongue(Transform parent, Vector3 pos, Vector3 forward)
        {
            var tongue = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tongue.name = "Tongue";
            tongue.transform.SetParent(parent);
            tongue.transform.position = pos + forward * 0.08f;
            tongue.transform.rotation = Quaternion.FromToRotation(Vector3.up, forward);
            tongue.transform.localScale = new Vector3(0.030f, 0.10f, 0.030f);
            Object.Destroy(tongue.GetComponent<CapsuleCollider>());
            tongue.GetComponent<Renderer>().sharedMaterial = AcquireTongueMat();

            Vector3 perp = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 prongBase = pos + forward * 0.18f;
            CreateTongueProng(parent, prongBase + perp * 0.025f, forward + perp * 0.8f);
            CreateTongueProng(parent, prongBase - perp * 0.025f, forward - perp * 0.8f);
        }

        private static void CreateTongueProng(Transform parent, Vector3 pos, Vector3 forward)
        {
            var prong = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prong.name = "TongueFork";
            prong.transform.SetParent(parent);
            prong.transform.position = pos + forward * 0.06f;
            prong.transform.rotation = Quaternion.FromToRotation(Vector3.up, forward);
            prong.transform.localScale = new Vector3(0.018f, 0.07f, 0.018f);
            Object.Destroy(prong.GetComponent<CapsuleCollider>());
            prong.GetComponent<Renderer>().sharedMaterial = AcquireProngMat();
        }
    }
}
