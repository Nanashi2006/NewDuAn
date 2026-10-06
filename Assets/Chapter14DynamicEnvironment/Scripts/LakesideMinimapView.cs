using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class LakesideMinimapView : MonoBehaviour
{
    private Camera mapCamera;
    private RenderTexture texture;
    private void Awake()
    {
        mapCamera = GetComponent<Camera>();
        texture = new RenderTexture(256, 256, 16) { name = "Lakeside Minimap" };
        mapCamera.targetTexture = texture;
        mapCamera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
    }
    private void OnGUI()
    {
        if (texture == null) return;
        float size = Mathf.Clamp(Screen.height * 0.23f, 120f, 220f);
        Rect rect = new Rect(Screen.width - size - 24f, Screen.height - size - 24f, size, size);
        Color old = GUI.color;
        GUI.color = new Color(0.85f, 0.69f, 0.39f);
        GUI.DrawTexture(new Rect(rect.x - 3f, rect.y - 3f, size + 6f, size + 6f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, false);
        GUI.color = new Color(1f, 0.76f, 0.18f);
        GUI.DrawTexture(new Rect(rect.center.x - 3, rect.center.y - 3, 6, 6), Texture2D.whiteTexture);
        GUI.color = old;
    }
    private void OnDestroy()
    {
        if (mapCamera != null) mapCamera.targetTexture = null;
        if (texture != null) { texture.Release(); Destroy(texture); }
    }
}
