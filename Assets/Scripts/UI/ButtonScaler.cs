using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
  public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
  {
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float tweenDuration = 0.1f;

    private Vector3 _originalScale;
    private Tween _scaleTween;

    private void Awake() => _originalScale = transform.localScale;

    public void OnPointerDown(PointerEventData eventData)
    {
      _scaleTween?.Kill();
      _scaleTween = transform.DOScale(_originalScale * pressedScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData) => Reset();

    public void OnPointerExit(PointerEventData eventData) => Reset();

    private void Reset()
    {
      _scaleTween?.Kill();
      _scaleTween = transform.DOScale(_originalScale, tweenDuration).SetEase(Ease.OutBack);
    }
  }
}