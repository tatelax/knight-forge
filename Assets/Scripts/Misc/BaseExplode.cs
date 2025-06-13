using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Misc
{
    public class BaseExplode : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float force = 1f;
        [SerializeField] private int despawnTime = 3000;
        [SerializeField] private float animSpeed = 0.5f;
        [SerializeField] private Ease ease;

        [Header("References")]
        [SerializeField] private Rigidbody[] pieces;

        private void Start()
        {
            for (var i = 0; i < pieces.Length; i++)
            {
                pieces[i].AddForce(Vector3.forward * force, ForceMode.Impulse);
            }
            
            DeSpawn().Forget();
        }

        private async UniTask DeSpawn()
        {
            await UniTask.Delay(despawnTime);

            gameObject.transform.DOScale(Vector3.zero, animSpeed).SetEase(ease).onComplete += () =>
            {
                Addressables.Release(transform.parent.gameObject);
            };
        }
    }
}
