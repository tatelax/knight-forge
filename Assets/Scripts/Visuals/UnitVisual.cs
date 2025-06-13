using Cysharp.Threading.Tasks;
using DG.Tweening;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Visuals
{
    public class UnitVisual : MonoBehaviour
    {
        private static readonly int State = Animator.StringToHash("UnitState");

        [Header("Settings")]
        [SerializeField] private Vector3 _punchDirection;
        [SerializeField] private float _punchTweenTime = 0.5f;
        [SerializeField] private int _punchTweenVibrato = 10;
        [SerializeField] private int _punchTweenElasticity = 10;
        
        [Header("References")]
        [SerializeField] private Slider _healthBar;
        [SerializeField] private GameObject _visual;
        [SerializeField] private GameObject _deadVisual;
        
        [Header("Optional References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _dustCloud;
        
        public Slider HealthBar => _healthBar;
        public Animator Animator => _animator;
        public GameObject Visual => _visual;
        public GameObject DeadVisual => _deadVisual;
        public ParticleSystem DustCloud => _dustCloud;

        private Tween _currTween;
        private Vector3 _visualScaleCache;

        private void Awake()
        {
            if(_deadVisual)
                _deadVisual.SetActive(false);
            
            if(_dustCloud)
                _dustCloud.Stop();

            _visualScaleCache = _visual.transform.localScale;
        }

        public void Attack()
        {
            _currTween?.Rewind();
            _currTween?.Kill();

            _currTween = _visual.transform.DOPunchScale(_punchDirection, _punchTweenTime, _punchTweenVibrato, _punchTweenElasticity);
        }

        public void Kill()
        {
            _healthBar.gameObject.SetActive(false);
            
            if (!_visual || !_deadVisual)
            {
                _animator.SetInteger(State, (int)UnitState.Dead);
                Release().Forget();
                return;
            }
            
            _visual.SetActive(false);
            _deadVisual.SetActive(true);
        }

        private async UniTask Release()
        {
            await UniTask.WaitForSeconds(3f);

            _visual.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InExpo).onComplete += () =>
            {
                Addressables.Release(gameObject);
            };
        }
    }
}
