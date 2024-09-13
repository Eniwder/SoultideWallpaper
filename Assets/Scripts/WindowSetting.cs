
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
public class WindowSetting : MonoBehaviour
{

    public RawImage backgroundImage; // 背景に設定する RawImage

    void Start()
    {
        Application.targetFrameRate = ConfigLoader.GetConfig().fps;
        StartCoroutine(LoadBackground());
    }

    private IEnumerator LoadBackground()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "background.png");

        // スマホやPCでのパス違いを考慮して UnityWebRequest を使用
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error loading background image: " + request.error);
        }
        else
        {
            // テクスチャを取得して、背景画像に設定
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            backgroundImage.texture = texture;
            backgroundImage.color = new Color(255, 255, 255, 1);
            FitToScreen(backgroundImage);
        }

    }

    private void FitToScreen(RawImage rawImage)
    {
        // RawImage の RectTransform を取得
        RectTransform rt = rawImage.rectTransform;

        // アンカーを画面全体に設定 (四隅にセット)
        rt.anchorMin = Vector2.zero; // 左下 (0, 0)
        rt.anchorMax = Vector2.one;  // 右上 (1, 1)

        // オフセットを0に設定して、画面全体にフィット
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

}

