using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Helpers;
using UniOrchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;

namespace Systems
{
  public class PlayerUnitPlacementSystem : ISystem
  {
    public (int x, int y) CurrentVisualTile { get; private set; }
    public (int x, int y) PreviousVisualTile { get; private set; }
    public UnitVisual CurrentUnitVisual { get; private set; }

    private const float DragHeight = 4.0f;
    private const float DragSpeed = 20.0f;

    private MapSystem _mapSystem;
    private PowerbarSystem _powerbarSystem;
    private AudioSystem _audioSystem;
    private UISystem _uiSystem;
    
    private Vector3 _targetPosition;
    private Vector3 _targetPositionLastFrame;
    
    public async UniTask Init()
    {
      _uiSystem = await Orchestrator.GetSystemAsync<UISystem>();
      _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
      _powerbarSystem = await Orchestrator.GetSystemAsync<PowerbarSystem>();
      _audioSystem = await Orchestrator.GetSystemAsync<AudioSystem>();
      
      _uiSystem.OnDragBegin += id => _ = OnDragBegin(id);
      _uiSystem.OnDragEnd += OnDragEnd;
    }

    private async UniTask OnDragBegin(string id)
    {
      if (CurrentUnitVisual is not null)
        return;

      var character = _uiSystem.CharacterButtons.First(b => b.UnitData.Name == id).UnitData;
      CurrentUnitVisual = await SpawnUnitHelper.SpawnVisual(character, GetDragPosInWorldSpace());

      CurrentUnitVisual.transform.DOScale(Vector3.one * 1.4f, 0.3f);
      
      _audioSystem.Play(Sound.Create);
    }
    
    private void OnDragEnd(string id)
    {
      if (CurrentUnitVisual is null)
        return;
      
      CurrentUnitVisual.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutExpo);
      CurrentUnitVisual.transform.DOMoveY(0, 0.2f).SetEase(Ease.OutExpo).onComplete += () =>
      {
        var characterData = _uiSystem.CharacterButtons.First(b => b.UnitData.Name == id).UnitData;
        var newUnit = _mapSystem.CreateUnit(CurrentUnitVisual, true, characterData);

        if (newUnit == null)
        {
          Addressables.Release(CurrentUnitVisual.gameObject);
          _audioSystem.Play(Sound.Delete);
        }
        else
        {
          _powerbarSystem.UsePower(characterData.PowerRequired);
          _audioSystem.Play(Sound.Drop);
        }

        CurrentUnitVisual = null;
      };
    }

    public void Update()
    {
      if (CurrentUnitVisual is null)
        return;

      _targetPosition = GetDragPosInWorldSpace(); 
      CurrentUnitVisual.transform.position = Vector3.Lerp(CurrentUnitVisual.transform.position, _targetPosition, Time.deltaTime * DragSpeed);
      
      if (_targetPosition != _targetPositionLastFrame)
      {
        _audioSystem.Play(Sound.ChangeTile);
      }

      _targetPositionLastFrame = _targetPosition;
    }

    private Vector3 GetDragPosInWorldSpace()
    {
      PreviousVisualTile = CurrentVisualTile;
      
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

      if (Physics.Raycast(ray, out var hit))
      {
        CurrentVisualTile = MapSystem.WorldToTileSpace(hit.point, MapSystem.SizeX - 1, (MapSystem.SizeY - 1) / 2);

        if (CurrentUnitVisual is null)
        {
          return Vector3.zero;
        }

        Vector3 snappedWorld = MapSystem.TileToWorldSpace(CurrentVisualTile);
        snappedWorld.y = DragHeight;
        return snappedWorld;
      }

      return CurrentUnitVisual is not null ? CurrentUnitVisual.transform.position : Vector3.zero;
    }
  }
}
