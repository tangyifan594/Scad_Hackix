using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public sealed class PaperPanelController : MonoBehaviour
{
    [SerializeField] RectTransform paperRoot;
    [SerializeField] Vector2 hiddenOffset = new Vector2(1200f, 0f);
    [SerializeField, Min(0.01f)] float slideDuration = 0.5f;
    [SerializeField] bool startHidden = true;
    [SerializeField] bool useUnscaledTime = true;
    [SerializeField] UnityEvent onSlideInFinished = new UnityEvent();
    [SerializeField] UnityEvent onSlideOutFinished = new UnityEvent();

    CanvasGroup canvasGroup;
    Vector2 shownPosition;
    Coroutine slideCoroutine;

    public bool IsShown { get; private set; }

    void Awake()
    {
        if(!paperRoot)paperRoot = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        shownPosition = paperRoot.anchoredPosition;

        if(startHidden)
            SetHiddenImmediately();
        else
            SetShownImmediately();
    }

    public void SlideIn()
    {
        StartSlide(shownPosition, true);
    }

    public void SlideOut()
    {
        StartSlide(shownPosition + hiddenOffset, false);
    }

    public void SetShownImmediately()
    {
        StopCurrentSlide();
        paperRoot.anchoredPosition = shownPosition;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        IsShown = true;
    }

    public void SetHiddenImmediately()
    {
        StopCurrentSlide();
        paperRoot.anchoredPosition = shownPosition + hiddenOffset;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        IsShown = false;
    }

    void StartSlide(Vector2 destination, bool showing)
    {
        StopCurrentSlide();
        gameObject.SetActive(true);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        slideCoroutine = StartCoroutine(Slide(destination, showing));
    }

    IEnumerator Slide(Vector2 destination, bool showing)
    {
        Vector2 start = paperRoot.anchoredPosition;
        float elapsed = 0f;

        while(elapsed < slideDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / slideDuration);
            float easedProgress = progress * progress * (3f - 2f * progress);
            paperRoot.anchoredPosition = Vector2.LerpUnclamped(start, destination, easedProgress);
            yield return null;
        }

        paperRoot.anchoredPosition = destination;
        IsShown = showing;
        canvasGroup.interactable = showing;
        canvasGroup.blocksRaycasts = showing;
        slideCoroutine = null;

        if(showing)onSlideInFinished.Invoke();
        else onSlideOutFinished.Invoke();
    }

    void StopCurrentSlide()
    {
        if(slideCoroutine == null)return;
        StopCoroutine(slideCoroutine);
        slideCoroutine = null;
    }
}
