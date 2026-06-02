using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float scaleFactor = 1.2f;
    public float animationSpeed = 0.1f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Smoothly interpolate to the target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleFactor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    private void OnDisable()
    {
        // Reset scale if the button is disabled to prevent it being stuck at large scale
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}
