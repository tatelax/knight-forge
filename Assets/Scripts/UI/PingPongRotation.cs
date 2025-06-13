using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  public class PingPongRotation : MonoBehaviour
  {
    [SerializeField] private Image _image;
    [SerializeField] private Vector3 _rotateAmount;
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _randomness;
    [SerializeField] private int _vibrato;
    [SerializeField] private bool _fadeOut;

    private void Start()
    {
      _image.rectTransform.DOShakeRotation(_rotateSpeed, _rotateAmount, _vibrato, _randomness, _fadeOut).SetLoops(-1);
    }
  }
}
