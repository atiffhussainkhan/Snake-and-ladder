using System.Collections.Generic;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class WoodTextureGen
    {
        private static Texture2D _woodA;
        private static Texture2D _woodB;
        private static Texture2D _mahogany;
        private static readonly Dictionary<Color, Texture2D> _scaleCache = new Dictionary<Color, Texture2D>();

        // Clears the cached scaled-texture variants so SnakeColors changes take effect immediately.
        public static void Invalidate()
        {
            _scaleCache.Clear();
        }

        public static Texture2D Wood(int size, Color baseColor, Color darkColor, float grainScale = 4f, float twist = 0.35f, float ringFreq = 8f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 4;
            var pixels = new Color[size * size];
            float inv = 1f / size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x * inv;
                    float v = y * inv;
                    float warp = Mathf.PerlinNoise(v * ringFreq, u * 1.2f) * twist;
                    float ring = Mathf.Sin((v + warp) * ringFreq * Mathf.PI);
                    ring = ring * 0.5f + 0.5f;
                    ring = Mathf.Pow(ring, 1.4f);
                    float fine = Mathf.PerlinNoise(u * 28f, v * 4f) * 0.18f;
                    float k = Mathf.Clamp01(ring * 0.85f + fine);
                    pixels[y * size + x] = Color.Lerp(baseColor, darkColor, k);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }

        public static Texture2D WoodNormal(int size, float strength = 1.5f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color[size * size];
            float inv = 1f / size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x * inv;
                    float v = y * inv;
                    float h = Mathf.PerlinNoise(u * 4f, v * 0.7f);
                    float dhx = (Mathf.PerlinNoise((u + inv) * 4f, v * 0.7f) - h) * strength;
                    float dhy = (Mathf.PerlinNoise(u * 4f, (v + inv) * 0.7f) - h) * strength;
                    pixels[y * size + x] = EncodeNormal(new Vector3(-dhx, -dhy, 1f));
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }

        public static Texture2D SnakeScale(int size, Color bodyColor, Color darkColor)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 4;
            var pixels = new Color[size * size];
            float inv = 1f / size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x * inv;
                    float v = y * inv;
                    float row = Mathf.Floor(v * 12f);
                    float offset = (Mathf.FloorToInt(row) % 2) * 0.5f;
                    float fx = Mathf.Repeat((u + offset) * 12f, 1f) - 0.5f;
                    float fy = Mathf.Repeat(v * 12f, 1f) - 0.5f;
                    float d = Mathf.Sqrt((fx + 0.5f) * (fx + 0.5f) + (fy + 0.5f) * (fy + 0.5f)) * 2f;
                    float bright = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);
                    float shade = bright * (1f - Mathf.SmoothStep(0.65f, 0.95f, d));
                    Color c = Color.Lerp(darkColor, bodyColor, shade);
                    float n = Mathf.PerlinNoise(u * 80f, v * 80f) * 0.06f;
                    c.r = Mathf.Clamp01(c.r + n);
                    c.g = Mathf.Clamp01(c.g + n);
                    c.b = Mathf.Clamp01(c.b + n);
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }

        public static Texture2D ScaleNormal(int size, float strength = 1.4f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 4;
            var pixels = new Color[size * size];
            float inv = 1f / size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x * inv;
                    float v = y * inv;
                    float row = Mathf.Floor(v * 12f);
                    float offset = (Mathf.FloorToInt(row) % 2) * 0.5f;
                    float fx = Mathf.Repeat((u + offset) * 12f, 1f) - 0.5f;
                    float fy = Mathf.Repeat(v * 12f, 1f) - 0.5f;
                    float r2 = Mathf.Sqrt(Mathf.Clamp01(1f - (fx * fx + fy * fy) * 4f));
                    pixels[y * size + x] = EncodeNormal(new Vector3(fx * 2f * strength, fy * 2f * strength, r2));
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(true, true);
            return tex;
        }

        public static Texture2D GetWoodA()    => _woodA    ?? (_woodA    = Wood(256, new Color(0.68f, 0.48f, 0.26f), new Color(0.32f, 0.20f, 0.10f), 3.2f, 0.30f, 6f));
        public static Texture2D GetWoodB()    => _woodB    ?? (_woodB    = Wood(256, new Color(0.55f, 0.36f, 0.18f), new Color(0.22f, 0.12f, 0.06f), 4.0f, 0.40f, 9f));
        public static Texture2D GetMahogany() => _mahogany ?? (_mahogany = Wood(256, new Color(0.46f, 0.24f, 0.12f), new Color(0.18f, 0.08f, 0.04f), 5.0f, 0.25f, 12f));

        public static Texture2D GetScale(Color c)
        {
            Color key = new Color(Mathf.Round(c.r * 4f) / 4f, Mathf.Round(c.g * 4f) / 4f, Mathf.Round(c.b * 4f) / 4f);
            if (!_scaleCache.TryGetValue(key, out var tex))
            {
                // Snake scale textures are heavily tiled (4×10), so 128² is visually indistinguishable from 256².
                tex = SnakeScale(128, key, key * 0.55f);
                _scaleCache[key] = tex;
            }
            return tex;
        }

        // Destroys every procedurally-generated texture and clears the cache. Safe to call multiple times.
        public static void Cleanup()
        {
            if (_woodA != null)    { Object.Destroy(_woodA);    _woodA = null; }
            if (_woodB != null)    { Object.Destroy(_woodB);    _woodB = null; }
            if (_mahogany != null) { Object.Destroy(_mahogany); _mahogany = null; }
            foreach (var tex in _scaleCache.Values) if (tex != null) Object.Destroy(tex);
            _scaleCache.Clear();
        }

        private static Color EncodeNormal(Vector3 n)
        {
            n = n.normalized;
            return new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
        }
    }
}
