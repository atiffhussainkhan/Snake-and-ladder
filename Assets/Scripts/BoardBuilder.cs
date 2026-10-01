using System.Collections.Generic;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class BoardBuilder
    {
        public const int SIZE = 10;
        public const float TILE = 1.0f;

        public static readonly Dictionary<int, int> Snakes = new Dictionary<int, int>
        {
            { 99, 54 }, { 70, 55 }, { 52, 42 }, { 25, 2 }, { 95, 72 }, { 64, 18 }
        };

        public static readonly Dictionary<int, int> Ladders = new Dictionary<int, int>
        {
            { 4, 14 }, { 9, 31 }, { 21, 39 }, { 28, 56 }, { 36, 57 }, { 51, 67 }, { 62, 81 }, { 71, 91 }
        };

        // Shared across all 100 tiles — per-tile colour tint is applied via MaterialPropertyBlock.
        private static Material _tileMat;
        private static Material _grooveMat;
        private static Material _frameMat;
        private static Material _baseMat;
        private static MaterialPropertyBlock _tileMpb;
        private static Font _legacyFont;

        public static void BuildBoard()
        {
            var board = new GameObject("Board");
            board.transform.position = Vector3.zero;

            CreateBaseplate(board.transform);
            CreateBeveledTiles(board.transform);
            CreateFrame(board.transform);
            CreateTileNumbers(board.transform);
        }

        public static int SquareFromRowCol(int row, int col)
        {
            if (row % 2 == 0) return row * SIZE + col + 1;
            return row * SIZE + (SIZE - 1 - col) + 1;
        }

        public static Vector3 PositionForSquare(int square)
        {
            if (square < 1 || square > 100)
            {
                Debug.LogError($"PositionForSquare: square {square} is out of range");
                return Vector3.zero;
            }
            int zero = square - 1;
            int row = zero / SIZE;
            int col = row % 2 == 0 ? zero % SIZE : SIZE - 1 - (zero % SIZE);
            return new Vector3(col * TILE, 0.55f, row * TILE);
        }

        public static int ResolveSquare(int square)
        {
            if (Snakes.ContainsKey(square)) return Snakes[square];
            if (Ladders.ContainsKey(square)) return Ladders[square];
            return square;
        }

        // Destroys every cached material. Safe to call multiple times.
        public static void Cleanup()
        {
            if (_tileMat   != null) { Object.Destroy(_tileMat);   _tileMat = null; }
            if (_grooveMat != null) { Object.Destroy(_grooveMat); _grooveMat = null; }
            if (_frameMat  != null) { Object.Destroy(_frameMat);  _frameMat = null; }
            if (_baseMat   != null) { Object.Destroy(_baseMat);   _baseMat = null; }
            _tileMpb = null;
        }

        private static void CreateBaseplate(Transform parent)
        {
            var baseplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseplate.name = "BoardBase";
            baseplate.transform.SetParent(parent);
            baseplate.transform.localScale = new Vector3(SIZE * TILE + 1.4f, 0.50f, SIZE * TILE + 1.4f);
            baseplate.transform.position = new Vector3((SIZE - 1) * TILE / 2f, -0.40f, (SIZE - 1) * TILE / 2f);
            baseplate.GetComponent<Renderer>().sharedMaterial = AcquireBaseMat();
            Object.Destroy(baseplate.GetComponent<BoxCollider>());
        }

        private static Material AcquireBaseMat()
        {
            if (_baseMat != null) return _baseMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.30f, 0.18f, 0.10f);
            mat.SetFloat("_Glossiness", 0.40f);
            mat.SetFloat("_Metallic", 0.05f);
            mat.mainTexture = WoodTextureGen.GetWoodB();
            mat.SetTextureScale("_MainTex", new Vector2(2f, 2f));
            _baseMat = mat;
            return mat;
        }

        private static void CreateBeveledTiles(Transform parent)
        {
            const float tileSize   = 0.86f;
            const float tileHeight = 0.18f;
            const float tileY      = -0.05f + tileHeight * 0.5f;

            var sharedMat = AcquireTileMat();
            if (_tileMpb == null) _tileMpb = new MaterialPropertyBlock();

            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Tile_{row}_{col}";
                    tile.transform.SetParent(parent);
                    tile.transform.localScale = new Vector3(tileSize, tileHeight, tileSize);
                    tile.transform.position = new Vector3(col * TILE, tileY, row * TILE);
                    Object.Destroy(tile.GetComponent<BoxCollider>());

                    Color tint = new Color(0.92f, 0.78f, 0.52f);
                    if (row % 2 == 1) tint *= 0.94f;
                    if (col % 2 == 1) tint *= 0.97f;

                    var renderer = tile.GetComponent<Renderer>();
                    renderer.sharedMaterial = sharedMat;
                    _tileMpb.SetColor("_Color", tint);
                    renderer.SetPropertyBlock(_tileMpb);
                }
            }
        }

        private static Material AcquireTileMat()
        {
            if (_tileMat != null) return _tileMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Glossiness", 0.45f);
            mat.SetFloat("_Metallic", 0.03f);
            mat.mainTexture = WoodTextureGen.GetWoodA();
            mat.SetTextureScale("_MainTex", new Vector2(1.2f, 1.2f));
            mat.SetTexture("_BumpMap", WoodTextureGen.WoodNormal(256, 0.7f));
            mat.SetFloat("_BumpScale", 0.4f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.02f, 0.01f, 0.005f));
            _tileMat = mat;
            return mat;
        }

        private static void CreateFrame(Transform parent)
        {
            const float fT = 0.45f;
            const float fH = 0.30f;
            float mid = (SIZE - 1) * TILE / 2f;
            CreateFrameStrip(parent, new Vector3(mid, fH/2f - 0.05f, -0.5f - fT/2f),                new Vector3(SIZE * TILE + fT*2f, fH, fT));
            CreateFrameStrip(parent, new Vector3(mid, fH/2f - 0.05f,  SIZE * TILE - 0.5f + fT/2f), new Vector3(SIZE * TILE + fT*2f, fH, fT));
            CreateFrameStrip(parent, new Vector3(-0.5f - fT/2f, fH/2f - 0.05f, mid),                    new Vector3(fT, fH, SIZE * TILE));
            CreateFrameStrip(parent, new Vector3( SIZE * TILE - 0.5f + fT/2f, fH/2f - 0.05f, mid), new Vector3(fT, fH, SIZE * TILE));
        }

        private static void CreateFrameStrip(Transform parent, Vector3 pos, Vector3 scale)
        {
            var f = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f.name = "Frame";
            f.transform.SetParent(parent);
            f.transform.position = pos;
            f.transform.localScale = scale;
            Object.Destroy(f.GetComponent<BoxCollider>());
            f.GetComponent<Renderer>().sharedMaterial = AcquireFrameMat();
        }

        private static Material AcquireFrameMat()
        {
            if (_frameMat != null) return _frameMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.42f, 0.24f, 0.10f);
            mat.SetFloat("_Glossiness", 0.45f);
            mat.SetFloat("_Metallic", 0.05f);
            mat.mainTexture = WoodTextureGen.GetMahogany();
            mat.SetTextureScale("_MainTex", new Vector2(4f, 0.6f));
            mat.SetTexture("_BumpMap", WoodTextureGen.WoodNormal(256, 1.2f));
            mat.SetFloat("_BumpScale", 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.06f, 0.03f, 0.01f));
            _frameMat = mat;
            return mat;
        }

        private static void CreateTileNumbers(Transform parent)
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    CreateTileNumber(parent, row, col, SquareFromRowCol(row, col));
                }
            }
        }

        // Number sits flush with the tile top surface (carved-in look) with a thin slightly-darker
        // shadow disc beneath it to read as an engraved groove.
        private static void CreateTileNumber(Transform parent, int row, int col, int square)
        {
            const float surfaceY = 0.18f;

            var groove = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groove.name = "TileGroove";
            groove.transform.SetParent(parent);
            groove.transform.position = new Vector3(col * TILE, surfaceY - 0.01f, row * TILE);
            groove.transform.localScale = new Vector3(0.62f, 0.005f, 0.62f);
            Object.Destroy(groove.GetComponent<BoxCollider>());
            groove.GetComponent<Renderer>().sharedMaterial = AcquireGrooveMat();

            var labelGo = new GameObject($"TileEmboss_{square}");
            labelGo.transform.SetParent(parent);
            labelGo.transform.position = new Vector3(col * TILE, surfaceY, row * TILE);
            labelGo.transform.rotation = Quaternion.Euler(90, 0, 0);
            var text = labelGo.AddComponent<TextMesh>();
            text.text = square.ToString();
            text.fontSize = 110;
            text.characterSize = 0.020f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(0.16f, 0.08f, 0.03f);
            text.fontStyle = FontStyle.Bold;
            if (_legacyFont == null) _legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = _legacyFont;
        }

        private static Material AcquireGrooveMat()
        {
            if (_grooveMat != null) return _grooveMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.78f, 0.56f, 0.30f);
            mat.SetFloat("_Glossiness", 0.35f);
            mat.SetFloat("_Metallic", 0.02f);
            _grooveMat = mat;
            return mat;
        }
    }
}
