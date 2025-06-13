using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;

namespace Helpers
{
  public static class SpawnUnitHelper
  {
    public static async UniTask<UnitVisual> SpawnVisual(UnitDataScriptableObject data, Vector3 pos)
    {
      var newGameObject = await Addressables.InstantiateAsync(data.AssetReference, pos, Quaternion.identity);

      if (newGameObject.TryGetComponent(out UnitVisual unitVisual))
      {
        var localScale = unitVisual.Visual.transform.localScale;
        unitVisual.Visual.transform.localScale = Vector3.zero;
        unitVisual.Visual.transform.DOScale(localScale, 0.2f);
        return unitVisual;
      }

      Addressables.Release(newGameObject);
      Debug.LogError("Can't find UnitVisual component");
      return null;
    }
  }
}