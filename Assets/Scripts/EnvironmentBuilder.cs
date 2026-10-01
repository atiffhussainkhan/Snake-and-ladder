using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class EnvironmentBuilder
    {
        private static Material _tableMat;

        public static void BuildEnvironment()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Table";
            ground.transform.position = new Vector3(BoardBuilder.SIZE * BoardBuilder.TILE / 2f - 0.5f, -0.50f, BoardBuilder.SIZE * BoardBuilder.TILE / 2f - 0.5f);
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            Object.Destroy(ground.GetComponent<MeshCollider>());
            ground.GetComponent<Renderer>().sharedMaterial = AcquireTableMat();

            RenderSettings.fog = false;
            RenderSettings.skybox = null;
        }

        public static void Cleanup()
        {
            if (_tableMat != null) { Object.Destroy(_tableMat); _tableMat = null; }
        }

        private static Material AcquireTableMat()
        {
            if (_tableMat != null) return _tableMat;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.86f, 0.68f, 0.48f);
            mat.SetFloat("_Glossiness", 0.10f);
            mat.SetFloat("_Metallic", 0.0f);
            mat.mainTexture = WoodTextureGen.GetWoodA();
            mat.SetTextureScale("_MainTex", new Vector2(6f, 6f));
            mat.SetTexture("_BumpMap", WoodTextureGen.WoodNormal(256, 0.7f));
            mat.SetFloat("_BumpScale", 0.4f);
            _tableMat = mat;
            return mat;
        }
    }
}
