using UnityEngine;

namespace SnakeLadder.Prototype
{
    public static class CameraSetup
    {
        public static void ConfigureCamera()
        {
            var camGo = Camera.main != null ? Camera.main.gameObject : NewMainCamera();
            var cam = camGo.GetComponent<Camera>();

            // Classic 3/4 board-game view: high enough to see all 100 squares, angled for 3D feel.
            cam.transform.position = new Vector3(5.5f, 9.0f, -5.5f);
            cam.transform.rotation = Quaternion.Euler(45, 0, 0);
            cam.fieldOfView = 38f;
            cam.backgroundColor = new Color(0.52f, 0.68f, 0.84f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.farClipPlane = 200f;

            // Scene's default Directional Light would double-up with our key/fill pair, so disable it.
            foreach (var sceneLight in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (sceneLight.type == LightType.Directional && sceneLight.gameObject.name == "Directional Light")
                {
                    sceneLight.enabled = false;
                }
            }

            CreateDirectionalLight("KeyLight",  new Color(1.00f, 0.96f, 0.86f), 1.05f, Quaternion.Euler(45, 35, 0),   LightShadows.Soft);
            CreateDirectionalLight("FillLight", new Color(0.78f, 0.86f, 1.00f), 0.40f, Quaternion.Euler(40, -130, 0), LightShadows.None);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.50f, 0.55f);
        }

        private static GameObject NewMainCamera()
        {
            var go = new GameObject("MainCamera");
            go.AddComponent<Camera>();
            go.tag = "MainCamera";
            if (Object.FindAnyObjectByType<AudioListener>() == null)
            {
                go.AddComponent<AudioListener>();
            }
            return go;
        }

        private static void CreateDirectionalLight(string name, Color color, float intensity, Quaternion rotation, LightShadows shadows)
        {
            var go = new GameObject(name);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = shadows;
            go.transform.rotation = rotation;
        }
    }
}
