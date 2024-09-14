using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(ScreenshotTaker))]
public sealed class ScreenshotTakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Go!"))
        {
            var screenshotTaker = (ScreenshotTaker)target;

            for (int i = 0; i < screenshotTaker.Items.Length; i++)
            {
                var targetItem = screenshotTaker.Items[i];

                foreach (var item in screenshotTaker.Items)
                {
                    item.SetActive(targetItem == item);
                }

                var camera = FindObjectOfType<Camera>();
                var filePath = EditorUtility.SaveFilePanel("Where to save?", string.Empty, targetItem.name, "png");

                TakeTransparentScreenshot(camera, 512, 512, filePath);
                AssetDatabase.Refresh();
            }
        }
    }

    public static void TakeTransparentScreenshot(Camera mainCam, int width, int height, string savePath)
    {
        // Create black and white cameras
        var blackCam = CreateCamera(mainCam, Color.black);
        var whiteCam = CreateCamera(mainCam, Color.white);

        // Set up textures
        var textureBlack = new Texture2D(width, height, TextureFormat.RGB24, false);
        var textureWhite = new Texture2D(width, height, TextureFormat.RGB24, false);
        var textureTransparentBackground = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grabArea = new Rect(0, 0, width, height);

        // Render black camera
        RenderCameraToTexture(blackCam, renderTexture, textureBlack, grabArea);

        // Render white camera
        RenderCameraToTexture(whiteCam, renderTexture, textureWhite, grabArea);

        // Combine textures to calculate alpha
        CalculateTransparentTexture(textureBlack, textureWhite, textureTransparentBackground);

        // Save PNG
        byte[] pngShot = textureTransparentBackground.EncodeToPNG();
        File.WriteAllBytes(savePath, pngShot);

        // Cleanup
        RenderTexture.ReleaseTemporary(renderTexture);
        DestroyImmediate(textureBlack);
        DestroyImmediate(textureWhite);
        DestroyImmediate(textureTransparentBackground);
        DestroyImmediate(blackCam.gameObject);
        DestroyImmediate(whiteCam.gameObject);
    }

    private static Camera CreateCamera(Camera mainCam, Color backgroundColor)
    {
        var camObject = new GameObject();
        var cam = camObject.AddComponent<Camera>();
        cam.CopyFrom(mainCam);
        cam.backgroundColor = backgroundColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObject.transform.SetParent(mainCam.transform, true);
        return cam;
    }

    private static void RenderCameraToTexture(Camera cam, RenderTexture renderTexture, Texture2D texture, Rect grabArea)
    {
        cam.targetTexture = renderTexture;
        cam.Render();
        RenderTexture.active = renderTexture;
        texture.ReadPixels(grabArea, 0, 0);
        texture.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
    }

    private static void CalculateTransparentTexture(Texture2D textureBlack, Texture2D textureWhite, Texture2D textureTransparentBackground)
    {
        for (int y = 0; y < textureTransparentBackground.height; ++y)
        {
            for (int x = 0; x < textureTransparentBackground.width; ++x)
            {
                float alpha = textureWhite.GetPixel(x, y).r - textureBlack.GetPixel(x, y).r;
                alpha = 1.0f - alpha;
                Color color;
                if (alpha == 0)
                {
                    color = Color.clear;
                }
                else
                {
                    color = textureBlack.GetPixel(x, y) / alpha;
                }
                color.a = alpha;
                textureTransparentBackground.SetPixel(x, y, color);
            }
        }
        textureTransparentBackground.Apply();
    }
}