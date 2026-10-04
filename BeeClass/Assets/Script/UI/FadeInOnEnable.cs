using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class FadeInOnEnable : MonoBehaviour
{
    [SerializeField, Min(0.01f)] float duration = 0.5f;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] bool disableInteractionDuringFade = true;

    CanvasGroup canvasGroup;
    Coroutine fadeCoroutine;
    bool previousInteractable;
    bool previousBlocksRaycasts;

    public void Configure(float fadeDuration)
    {
        duration = Mathf.Max(0.01f, fadeDuration);
    }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if(!canvasGroup)canvasGroup = GetComponent<CanvasGroup>();

        // Keep the UI fully visible while editing the scene.
        if(!Application.isPlaying)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        previousInteractable = canvasGroup.interactable;
        previousBlocksRaycasts = canvasGroup.blocksRaycasts;

        canvasGroup.alpha = 0f;
        if(disableInteractionDuringFade)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    void OnDisable()
    {
        if(fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        if(disableInteractionDuringFade)
        {
            canvasGroup.interactable = previousInteractable;
            canvasGroup.blocksRaycasts = previousBlocksRaycasts;
        }

        fadeCoroutine = null;
    }
}
