using DG.Tweening;
using UniOrchestrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  public class LoadingScreen: MonoBehaviour
  {
    [Header("Settings")]
    [SerializeField] private float _fadeSpeed = 0.2f;
    
    [Header("References")]
    [SerializeField] private RectTransform _loadingScreen;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _statusText;

    private void Awake()
    {
      _loadingScreen.gameObject.SetActive(true);

      _statusText.text = UniOrchestrator.Orchestrator.Status.ToString();
      UniOrchestrator.Orchestrator.OnBootStatusChanged += OnBootStatusChanged;
    }

    private void OnBootStatusChanged(BootStatus status)
    {
      _statusText.text = status.ToString();

      if (status == BootStatus.Successful)
      {
        LoadingComplete();
      }
    }

    private void LoadingComplete()
    {
      _image.DOFade(0, _fadeSpeed).onComplete += () =>
      {
        _loadingScreen.gameObject.SetActive(false);
      };
    }

    private void OnValidate()
    {
      Debug.Assert(_loadingScreen is not null, "_loadingScreen is null");
      Debug.Assert(_statusText is not null, "_statusText is null");
    }
  }
}