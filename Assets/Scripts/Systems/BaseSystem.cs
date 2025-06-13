using Cysharp.Threading.Tasks;
using Helpers;
using UniOrchestrator;
using ScriptableObjects;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public class BaseSystem : ISystem
    {
        private const ushort NumBases = 2;
        private const string PlayerBaseConfigAddress = "Assets/Settings/Game/PlayerBase.asset";
        private const string EnemyBaseConfigAddress = "Assets/Settings/Game/EnemyBase.asset";

        private UnitDataScriptableObject _playerBaseConfig;
        private UnitDataScriptableObject _enemyBaseConfig;

        private MapSystem _mapSystem;
        private GameStateSystem _gameStateSystem;

        private ushort _playerBasesKilled;
        private ushort _enemyBasesKilled;

        public async UniTask Init()
        {
            _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
            _gameStateSystem = await Orchestrator.GetSystemAsync<GameStateSystem>();
            
            _playerBaseConfig = await Addressables.LoadAssetAsync<UnitDataScriptableObject>(PlayerBaseConfigAddress);
            _enemyBaseConfig = await Addressables.LoadAssetAsync<UnitDataScriptableObject>(EnemyBaseConfigAddress);
            
            _mapSystem.OnUnitKilled += OnUnitKilled;
            
            await PlaceBases();
        }

        private void OnUnitKilled(Unit unit)
        {
            if (unit.Data.UnitType != UnitType.Base)
                return;

            if (unit.IsPlayerOwned)
                _playerBasesKilled++;
            else
                _enemyBasesKilled++;

            if (_playerBasesKilled == NumBases)
                _gameStateSystem.FinishGame(WinType.EnemyWon);
            else if(_enemyBasesKilled == NumBases)
                _gameStateSystem.FinishGame(WinType.PlayerWon);
        }

        private async UniTask PlaceBases()
        {
            for (int i = 0; i < NumBases; i++)
            {
                int r = i % 2;
                int x = 2;
                int y = (MapSystem.SizeY - 4) * r + 2;
                var a = MapSystem.TileToWorldSpace((x, y));
                var b = MapSystem.TileToWorldSpace((MapSystem.SizeX - x, y));

                await CreateBase(a, r == 0);
                await CreateBase(b, r == 0);
            }
        }

        private async UniTask CreateBase(Vector3 pos, bool isPlayerOwned)
        {
            var option = isPlayerOwned ? _playerBaseConfig : _enemyBaseConfig;
            var visual = await SpawnUnitHelper.SpawnVisual(option, pos);
            _ = _mapSystem.CreateUnit(visual, isPlayerOwned, option);
        }
    }
}