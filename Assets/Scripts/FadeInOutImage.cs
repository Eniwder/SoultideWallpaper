using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOutImage : MonoBehaviour
{
    public Image image;
    public float fadeDuration = 1.0f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = image.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = 1; // Ensure alpha is set to 1 at the end
        image.color = color;
    }

    public IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = image.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            color.a = alpha;
            image.color = color;
            yield return null;
        }
        color.a = 0; // Ensure alpha is set to 0 at the end
        image.color = color;
    }
}
