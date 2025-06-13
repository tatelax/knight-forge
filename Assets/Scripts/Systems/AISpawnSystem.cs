using Cysharp.Threading.Tasks;
using Helpers;
using UniOrchestrator;
using Types;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class AISpawnSystem: ISystem
  {
    private const float SpawnInterval = 10;
    
    private MapSystem _mapSystem;
    private GameStateSystem _gameStateSystem;
    private CharacterButton[] _characters;

    private float _spawnTimer;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
      _gameStateSystem = await Orchestrator.GetSystemAsync<GameStateSystem>();
      
      var uiSystem = await Orchestrator.GetSystemAsync<UISystem>();

      _characters = uiSystem.CharacterButtons;
      
      _spawnTimer = 5f;
    }

    public void Update()
    {
      if (_gameStateSystem.State != GameState.Playing)
        return;
      
      _spawnTimer -= Time.deltaTime;

      if (_spawnTimer >= 0f)
        return;

      SpawnUnit().Forget();
      
      _spawnTimer = SpawnInterval;
    }

    private async UniTask SpawnUnit()
    {
      var random = Random.Range(0, _characters.Length - 1);
      (int, int) tile = SelectTile();
      var pos = MapSystem.TileToWorldSpace(tile);
      var character = _characters[random].UnitData;
      
      var newCharacter = await SpawnUnitHelper.SpawnVisual(character, pos);

      if (newCharacter is null)
        return;
      
      var newUnit = _mapSystem.CreateUnit(newCharacter, false, character);

      if (newUnit is null)
        Addressables.Release(newCharacter.Visual);
    }

    private (int, int) SelectTile()
    {
      return Random.Range(0f, 1f) > 0.5f ? (MapSystem.SizeX - 2, MapSystem.SizeY - 4) : (2, MapSystem.SizeY - 4);
    }
  }
}