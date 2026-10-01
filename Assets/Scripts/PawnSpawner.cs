using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class PawnSpawner
    {
        public static GameObject Pawn1 { get; private set; }
        public static GameObject Pawn2 { get; private set; }
        public static int Pawn1Square = 1;
        public static int Pawn2Square = 1;

        private static Material _redBodyMat;
        private static Material _redHeadMat;
        private static Material _blueBodyMat;
        private static Material _blueHeadMat;

        public static void SpawnPawns()
        {
            Pawn1 = CreatePawn("Pawn1_Red",  new Color(0.94f, 0.28f, 0.28f));
            Pawn2 = CreatePawn("Pawn2_Blue", new Color(0.22f, 0.55f, 0.96f));
            Pawn1.transform.position = BoardBuilder.PositionForSquare(1) + new Vector3(-0.28f, 0f,  0.28f);
            Pawn2.transform.position = BoardBuilder.PositionForSquare(1) + new Vector3( 0.28f, 0f, -0.28f);
        }

        public static void Cleanup()
        {
            if (_redBodyMat  != null) { Object.Destroy(_redBodyMat);  _redBodyMat = null; }
            if (_redHeadMat  != null) { Object.Destroy(_redHeadMat);  _redHeadMat = null; }
            if (_blueBodyMat != null) { Object.Destroy(_blueBodyMat); _blueBodyMat = null; }
            if (_blueHeadMat != null) { Object.Destroy(_blueHeadMat); _blueHeadMat = null; }
            Pawn1 = null;
            Pawn2 = null;
        }

        private static GameObject CreatePawn(string name, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.localScale = new Vector3(0.40f, 0.55f, 0.40f);
            Object.Destroy(go.GetComponent<CapsuleCollider>());
            go.GetComponent<Renderer>().sharedMaterial = AcquireBodyMat(color);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(go.transform);
            head.transform.localPosition = new Vector3(0, 0.85f, 0);
            head.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            Object.Destroy(head.GetComponent<SphereCollider>());
            head.GetComponent<Renderer>().sharedMaterial = AcquireHeadMat(color);

            return go;
        }

        private static Material AcquireBodyMat(Color color)
        {
            if (color.r > color.b) return _redBodyMat  ?? (_redBodyMat  = Make(color, 0.30f, 0.65f, 0.15f));
            return                       _blueBodyMat ?? (_blueBodyMat = Make(color, 0.30f, 0.65f, 0.15f));
        }

        private static Material AcquireHeadMat(Color color)
        {
            if (color.r > color.b) return _redHeadMat  ?? (_redHeadMat  = Make(color, 0.25f, 0.75f, 0.18f));
            return                       _blueHeadMat ?? (_blueHeadMat = Make(color, 0.25f, 0.75f, 0.18f));
        }

        private static Material Make(Color color, float metallic, float glossiness, float emissionScale)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", glossiness);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emissionScale);
            return mat;
        }

        // Resets pawn static state between tests
        public static void ResetForNewGame()
        {
            Pawn1Square = 1;
            Pawn2Square = 1;
            if (Pawn1 != null) Pawn1.transform.position = BoardBuilder.PositionForSquare(1) + new Vector3(-0.28f, 0f,  0.28f);
            if (Pawn2 != null) Pawn2.transform.position = BoardBuilder.PositionForSquare(1) + new Vector3( 0.28f, 0f, -0.28f);
        }
    }
}
